const backofficePath = './src/Preflight/Backoffice';

// two directories up to the test sites
// but build into /src

export const paths = {
    js: [`${backofficePath}/**/*.js`,],
    json: [`${backofficePath}/**/*.json`,],
    viewsDev: [`${backofficePath}/**/*.html`],
    viewsProd: [`${backofficePath}/**/*.html`, `!${backofficePath}/**/components/**/*.html`],
    scss: `${backofficePath}/**/*.scss`,
    lang: `./src/Preflight/Lang/*.xml`,
    manifest: './src/Preflight/package.manifest',
    dest: './App_Plugins/Prefligh/',
    site: '../../Preflight.Site.V8/App_Plugins/Preflight/',
    siteNetCore: '../../Preflight.Site.V9/App_Plugins/Preflight/',
};

export const config = {
    hash: new Date().toISOString().split('').reduce((a, b) => (((a << 5) - a) + b.charCodeAt(0)) | 0, 0).toString().substring(1)
};
