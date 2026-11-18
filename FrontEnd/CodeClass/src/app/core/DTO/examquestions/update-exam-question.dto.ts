export interface UpdateExamQuestionDTO {
  version: string;
  title: string;
  description: string;
  imagePath: string | null;
  maxScore: number;
  displayOrder: number;
}
