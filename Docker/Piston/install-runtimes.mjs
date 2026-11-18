import http from 'node:http';
import https from 'node:https';

const url = process.env.PISTON_URL ?? 'http://piston:2000';
const packages = [
  ['gcc', process.env.PISTON_CPP_VERSION],
  ['python', process.env.PISTON_PYTHON_VERSION],
  ['java', process.env.PISTON_JAVA_VERSION],
];

for (const [language, version] of packages) {
  if (!version) throw new Error(`Missing version for ${language}.`);
  const list = await fetch(`${url}/api/v2/packages`, { signal: AbortSignal.timeout(30000) });
  if (!list.ok) throw new Error('Could not read the Piston package catalog.');
  const catalog = await list.json();
  const runtime = catalog.find(item => item.language === language && item.language_version === version);
  if (!runtime) throw new Error(`${language} ${version} is not in the package catalog.`);
  if (runtime.installed) {
    console.log(`${language} ${version} is already installed.`);
    continue;
  }

  console.log(`Installing ${language} ${version}...`);
  await install(language, version);

}

const response = await fetch(`${url}/api/v2/runtimes`, { signal: AbortSignal.timeout(30000) });
if (!response.ok) throw new Error('Could not verify installed runtimes.');
const runtimes = await response.json();
for (const [language, version] of [['c++', process.env.PISTON_CPP_VERSION], ['python', process.env.PISTON_PYTHON_VERSION], ['java', process.env.PISTON_JAVA_VERSION]]) {
  if (!runtimes.some(runtime => runtime.language === language && runtime.version === version)) {
    throw new Error(`${language} ${version} was not installed successfully.`);
  }
}
console.log('C++, Python and Java runtimes are ready.');


// Large compiler downloads can exceed fetch's default response-header timeout.
function install(language, version) {
  return new Promise((resolve, reject) => {
    const endpoint = new URL('/api/v2/packages', url);
    const transport = endpoint.protocol === 'https:' ? https : http;
    const request = transport.request(endpoint, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      signal: AbortSignal.timeout(1800000),
    }, response => {
      response.resume();
      response.on('end', () => {
        if (response.statusCode >= 200 && response.statusCode < 300) resolve();
        else reject(new Error(`Could not install ${language} ${version} (HTTP ${response.statusCode}).`));
      });
      response.on('error', reject);
    });

    request.on('error', reject);
    request.end(JSON.stringify({ language, version }));
  });
}
