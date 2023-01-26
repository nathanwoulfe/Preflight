const backofficePath = './app';

function getOutputPath() {
  var argv = require('minimist')(process.argv.slice(2));

  var outputPath = argv['output-path'];
  if (!outputPath) {
    outputPath = require('./config.outputPath.js').outputPath;
  }

  return outputPath;
}

export const paths = {
  js: [`${backofficePath}/**/*.ts`,],
  viewsDev: [`${backofficePath}/**/*.html`],
  viewsProd: [`${backofficePath}/**/*.html`, `!${backofficePath}/**/components/**/*.html`],
  scss: `${backofficePath}/**/*.scss`,
  lang: `${backofficePath}/Lang/*.xml`,
  dest: getOutputPath()
};

export const config = {
  hash: new Date().toISOString().split('').reduce((a, b) => (((a << 5) - a) + b.charCodeAt(0)) | 0, 0).toString().substring(1)
};
