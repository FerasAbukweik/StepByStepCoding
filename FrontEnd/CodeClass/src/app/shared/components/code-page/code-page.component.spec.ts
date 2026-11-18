import { DialogService } from '../../../core/services/local/dialog-service';
import { applyCodeChanges } from '../../Utilities/code-changes';
import { Component, input, output, signal } from '@angular/core';
import { TestBed, ComponentFixture } from '@angular/core/testing';
import { By } from '@angular/platform-browser';
import { Router } from '@angular/router';
import { Subject, of } from 'rxjs';
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';
import { CodePageComponent } from './code-page.component';
import { CodeEditorComponent } from '../code-editor/code-editor.component';
import { EditorChange } from '../code-editor/editor-change.model';
import { AuthService } from '../../../core/services/data/auth-service';
import { FilesApiService } from '../../../core/services/api/files-api-service';
import { SignalRService } from '../../../core/services/signalr/signalr-service';
import { FilesService } from '../../../core/services/data/files-service';
import { FileDetailDTO } from '../../../core/DTO/files/file-detail.dto';
import { CodeChangesDTO } from '../../../core/DTO/realtime/code-changes.dto';
import { SaveChangesDTO } from '../../../core/DTO/files/save-changes.dto';

@Component({ selector: 'app-code-editor', template: '' })
class EditorStub {
  readonly code = input('');
  readonly language = input('cpp');
  readonly readOnly = input(false);
  readonly changed = output<EditorChange>();
}

describe('CodePageComponent', () => {
  let fixture: ComponentFixture<CodePageComponent>;
  let current: FileDetailDTO;
  const dialogs = { confirm: vi.fn(async () => true) };
  let hub: ReturnType<typeof makeHub>;
  let api: { get: ReturnType<typeof vi.fn>; run: ReturnType<typeof vi.fn> };

  function makeHub() {
    return {
      changes$: new Subject<CodeChangesDTO>(),
      reconnected$: new Subject<void>(),
      lectureEnded$: new Subject<string>(),
      fileDeleted$: new Subject<string>(),
      state: signal('connected'),
      join: vi.fn(async () => {}),
      leave: vi.fn(async () => {}),
      save: vi.fn(async (_id: string, _request: SaveChangesDTO) => {}),
    };
  }

  beforeEach(async () => {
    current = {
      id: 'file',
      userId: 'user',
      lectureId: null,
      fileName: 'main.cpp',
      language: 1,
      savedContent: '',
      savedRevision: 0,
      version: 'version',
    };
    hub = makeHub();
    dialogs.confirm.mockReset().mockResolvedValue(true);
    api = {
      get: vi.fn(() => of({ ...current })),
      run: vi.fn(() =>
        of({
          language: 'c++',
          version: '10.2.0',
          compile: null,
          run: {
            stdout: '42',
            stderr: '',
            exitCode: 0,
            signal: null,
            status: null,
            message: null,
            wallTimeMilliseconds: 1,
          },
        }),
      ),
    };
    await TestBed.configureTestingModule({
      imports: [CodePageComponent],
      providers: [
        {
          provide: AuthService,
          useValue: { user: signal({ id: 'user', fileId: 'file' }), canTeach: () => false },
        },
        { provide: SignalRService, useValue: hub },
        { provide: DialogService, useValue: dialogs },
        { provide: FilesApiService, useValue: api },
        { provide: Router, useValue: { navigateByUrl: vi.fn(async () => true) } },
      ],
    })
      .overrideComponent(CodePageComponent, {
        remove: { imports: [CodeEditorComponent] },
        add: { imports: [EditorStub] },
      })
      .compileComponents();
    fixture = TestBed.createComponent(CodePageComponent);
    fixture.componentRef.setInput('fileId', 'file');
    fixture.detectChanges();
    await fixture.whenStable();
    fixture.detectChanges();
    await vi.waitFor(() => {
      fixture.detectChanges();
      expect(fixture.debugElement.query(By.directive(EditorStub))).not.toBeNull();
    });
  });

  afterEach(() => {
    if (!fixture.componentRef.hostView.destroyed) fixture.destroy();
    vi.restoreAllMocks();
  });

  function editor(): EditorStub {
    return fixture.debugElement.query(By.directive(EditorStub)).componentInstance;
  }

  it('runs the current code and displays output without changing the saved file', () => {
    const button = Array.from(
      fixture.nativeElement.querySelectorAll('button') as NodeListOf<HTMLButtonElement>,
    ).find((item) => item.textContent?.includes('Run code'))!;
    expect(button).toBeTruthy();
    button.click();
    fixture.detectChanges();
    expect(api.run).toHaveBeenCalledWith('file', { code: '', stdin: '' });
    expect(fixture.nativeElement.textContent).toContain('42');
    expect(hub.save).not.toHaveBeenCalled();
    expect(TestBed.inject(FilesService).file()?.savedRevision).toBe(0);
  });

  it('loads the file in ngOnInit and delegates subsequent reads to the API service', () => {
    expect(hub.join).toHaveBeenCalledWith('file');
    expect(api.get).toHaveBeenCalledOnce();
    expect(editor().readOnly()).toBe(false);
    expect(TestBed.inject(FilesService).file()?.fileName).toBe('main.cpp');
    TestBed.inject(FilesService).get('file').subscribe();
    expect(api.get).toHaveBeenCalledTimes(2);
  });

  it('saves a student-owned file and applies the sender broadcast only once', async () => {
    hub.save.mockImplementation(async (id, request) => {
      hub.changes$.next({
        fileId: id,
        lectureId: null,
        batchId: request.batchId,
        baseRevision: request.baseRevision,
        revision: request.baseRevision + 1,
        changeSets: request.changeSets,
      });
    });
    editor().changed.emit({
      code: 'hello',
      changeSet: { changes: [{ rangeOffset: 0, rangeLength: 0, text: 'hello' }] },
    });
    await fixture.componentInstance.save();
    fixture.detectChanges();

    expect(hub.save).toHaveBeenCalledOnce();
    expect(editor().code()).toBe('hello');
    expect(TestBed.inject(FilesService).file()?.savedContent).toBe('hello');
    expect(fixture.nativeElement.textContent).not.toContain('Unsaved changes');
  });

  it('keeps another users file read-only', async () => {
    current = { ...current, userId: 'someone-else' };
    await fixture.componentInstance.load();
    fixture.detectChanges();
    expect(editor().readOnly()).toBe(true);
    editor().changed.emit({
      code: 'forbidden',
      changeSet: { changes: [{ rangeOffset: 0, rangeLength: 0, text: 'forbidden' }] },
    });
    await fixture.componentInstance.save();
    expect(hub.save).not.toHaveBeenCalled();
  });

  it('saves rapid typing in ordered batches within the server limit', async () => {
    hub.save.mockImplementation(async (_id, request) => {
      current = {
        ...current,
        savedContent: applyCodeChanges(current.savedContent, request.changeSets),
        savedRevision: current.savedRevision + 1,
      };
      hub.changes$.next({
        fileId: current.id,
        lectureId: null,
        batchId: request.batchId,
        baseRevision: request.baseRevision,
        revision: current.savedRevision,
        changeSets: request.changeSets,
      });
    });
    for (let index = 0; index < 130; index++) {
      editor().changed.emit({
        code: 'x'.repeat(index + 1),
        changeSet: { changes: [{ rangeOffset: index, rangeLength: 0, text: 'x' }] },
      });
    }

    await fixture.componentInstance.save();
    await vi.waitFor(() => expect(fixture.componentInstance.hasUnsavedChanges()).toBe(false));
    expect(hub.save.mock.calls.map(([, request]) => request.changeSets.length)).toEqual([
      64, 64, 2,
    ]);
    expect(hub.save.mock.calls.map(([, request]) => request.baseRevision)).toEqual([0, 1, 2]);
    expect(current.savedContent).toBe('x'.repeat(130));
    fixture.detectChanges();
    expect(editor().code()).toBe(current.savedContent);
    expect(editor().readOnly()).toBe(false);
  });

  it('keeps the draft when the shared reload dialog is cancelled', async () => {
    editor().changed.emit({
      code: 'draft',
      changeSet: { changes: [{ rangeOffset: 0, rangeLength: 0, text: 'draft' }] },
    });
    dialogs.confirm.mockResolvedValue(false);

    await fixture.componentInstance.reload();
    fixture.detectChanges();

    expect(api.get).toHaveBeenCalledTimes(1);
    expect(editor().code()).toBe('draft');
    expect(fixture.componentInstance.hasUnsavedChanges()).toBe(true);
  });

  it('reloads the server version only after the shared dialog is confirmed', async () => {
    editor().changed.emit({
      code: 'draft',
      changeSet: { changes: [{ rangeOffset: 0, rangeLength: 0, text: 'draft' }] },
    });
    current = { ...current, savedContent: 'saved version', savedRevision: 1 };

    await fixture.componentInstance.reload();
    fixture.detectChanges();

    expect(dialogs.confirm).toHaveBeenCalledOnce();
    expect(api.get).toHaveBeenCalledTimes(2);
    expect(editor().code()).toBe('saved version');
    expect(fixture.componentInstance.hasUnsavedChanges()).toBe(false);
  });

  it('does not reload a page destroyed while its confirmation was open', async () => {
    editor().changed.emit({
      code: 'draft',
      changeSet: { changes: [{ rangeOffset: 0, rangeLength: 0, text: 'draft' }] },
    });
    let decide!: (value: boolean) => void;
    dialogs.confirm.mockImplementationOnce(
      () =>
        new Promise<boolean>((resolve) => {
          decide = resolve;
        }),
    );
    const pending = fixture.componentInstance.reload();
    fixture.destroy();

    decide(true);
    await pending;

    expect(api.get).toHaveBeenCalledTimes(1);
  });

  it('refetches after a revision gap and reconciles to the saved code', async () => {
    current = { ...current, savedContent: 'remote', savedRevision: 2 };
    hub.changes$.next({
      fileId: 'file',
      lectureId: null,
      batchId: 'remote',
      baseRevision: 1,
      revision: 2,
      changeSets: [],
    });
    await fixture.whenStable();
    fixture.detectChanges();
    expect(api.get).toHaveBeenCalledTimes(2);
    await vi.waitFor(() => {
      fixture.detectChanges();
      expect(editor().code()).toBe('remote');
    });
  });

  it('preserves an unsaved draft on reconnect instead of replaying stale offsets', async () => {
    editor().changed.emit({
      code: 'draft',
      changeSet: { changes: [{ rangeOffset: 0, rangeLength: 0, text: 'draft' }] },
    });
    current = { ...current, savedContent: 'other session', savedRevision: 1 };
    hub.reconnected$.next();
    await fixture.whenStable();
    fixture.detectChanges();
    expect(editor().code()).toBe('draft');
    await vi.waitFor(() => {
      fixture.detectChanges();
      expect(editor().readOnly()).toBe(true);
    });
    expect(fixture.nativeElement.textContent).toContain('Your draft is preserved');
    await fixture.componentInstance.save();
    expect(hub.save).not.toHaveBeenCalled();
  });
});
