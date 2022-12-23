const backofficePath = './app';

// temp => include UI component from Workflow.Core.LicensesValidation.Client

function getOutputPath() {
  var argv = require('minimist')(process.argv.slice(2));

  var outputPath = argv['output-path'];
  if (!outputPath) {
    outputPath = require('./config.outputPath.js').outputPath;
  }

  return outputPath;
}

export const paths = {
  js: [`${backofficePath}/**/*.ts`, `../Umbraco.Workflow.Core/LicenseValidation/Client/src/**/*.ts`, `!${backofficePath}/**/offline/**/*.ts`],
  offline: [`${backofficePath}/**/offline/**/*.ts`, `!${backofficePath}/**/offline/offline.loader.ts`],
  offlineLoader: `${backofficePath}/**/offline/offline.loader.ts`,
  viewsDev: [`${backofficePath}/**/*.html`],
  viewsProd: [`${backofficePath}/**/*.html`, `!${backofficePath}/**/components/**/*.html`],
  scss: `${backofficePath}/**/*.scss`,
  lang: `${backofficePath}/Lang/*.xml`,
  manifest: `${backofficePath}/**/package.manifest`,
  dest: getOutputPath()
};

export const config = {
  hash: new Date().toISOString().split('').reduce((a, b) => (((a << 5) - a) + b.charCodeAt(0)) | 0, 0).toString().substring(1)
};
