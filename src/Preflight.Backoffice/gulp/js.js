import gulp from 'gulp';
import globby from 'globby';
import through from 'through2';
import log from 'gulplog';
import sourcemaps from 'gulp-sourcemaps';
import babelify from 'babelify';
import browserify from 'browserify';
import source from 'vinyl-source-stream';
import buffer from 'vinyl-buffer';
import embedTemplates from 'gulp-angular-embed-templates';
import gulpif from 'gulp-if';

import { paths, config } from './config';

export function js() {
    return _js(paths.js, `preflight.js`);
} 

function _js(glob, filename) {
    const jsDest = 'Backoffice/js/';

    // gulp expects tasks to return a stream, so we create one here.
    var bundledStream = through();
    bundledStream
        // turns the output bundle stream into a stream containing
        // the normal attributes gulp plugins expect.
        .pipe(source(filename))

        // the rest of the gulp task, as you would normally write it.
        // here we're copying from the Browserify + Uglify2 recipe.
        .pipe(buffer())
        .pipe(sourcemaps.init({ loadMaps: true }))
        // Add gulp plugins to the pipeline here.
        .pipe(embedTemplates({
            basePath: './'
        }))
        .on('error', log.error)
        .pipe(sourcemaps.write('./'))
        .pipe(gulpif(!config.prod, gulp.dest(paths.siteNetCore + jsDest)))
        .pipe(gulpif(!config.prod, gulp.dest(paths.site + jsDest)))
        .pipe(gulpif(config.prod, gulp.dest(paths.dest + jsDest)));

    // "globby" replaces the normal "gulp.src" as Browserify
    // creates it's own readable stream.
    globby(glob).then(entries => {
        browserify({entries, debug: !config.prod, extensions: ['.ts', '.js']})
            //.plugin(tsify)
            .transform(babelify.configure({
                presets: ['@babel/preset-env']
            }))
            .bundle()
            .pipe(bundledStream);
    }).catch(err => bundledStream.emit('error', err));

    // finally, we return the stream, so gulp knows when this task is done.
    return bundledStream;
}
