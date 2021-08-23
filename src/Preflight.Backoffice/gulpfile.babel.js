import gulp from 'gulp';
import gulpif from 'gulp-if';

import { paths, config } from './gulp/config';
import { js } from './gulp/js';
import { clean } from './gulp/clean';
import { scss } from './gulp/scss';
import { views } from './gulp/views';

// set env from args
config.prod = process.argv.indexOf('--prod') > -1;

function manifest() {
    return gulp.src(paths.manifest)
        .pipe(gulpif(!config.prod, gulp.dest(paths.site)))
        .pipe(gulpif(!config.prod, gulp.dest(paths.siteNetCore)))
        .pipe(gulpif(config.prod, gulp.dest(paths.dest)));
};

function json() {
    const backoffice = 'Backoffice';
    return gulp.src(paths.json)
        .pipe(gulpif(!config.prod, gulp.dest(paths.site + backoffice)))
        .pipe(gulpif(!config.prod, gulp.dest(paths.siteNetCore + backoffice)))
        .pipe(gulpif(config.prod, gulp.dest(paths.dest + backoffice)));
};

function lang() {
    const lang = 'Lang';
    return gulp.src(paths.lang)
        .pipe(gulpif(!config.prod, gulp.dest(paths.site + lang)))
        .pipe(gulpif(!config.prod, gulp.dest(paths.siteNetCore + lang)))
        .pipe(gulpif(config.prod, gulp.dest(paths.dest + lang)));
};

// entry points... 
export const prod = gulp.task('prod',
    gulp.series(
        clean,
        gulp.parallel(
            js,
            json,
            scss,
            views,
            lang,
            manifest
        )));

export const dev = gulp.task('dev',
    gulp.series(
        clean,
        gulp.parallel(
            js,
            json,
            scss,
            views,
            lang,
            manifest,
            done => {

                console.log('watching for changes... ctrl+c to exit');
                gulp.watch(paths.js, gulp.series(js, views));
                gulp.watch(paths.scss, gulp.series(scss, views));
                gulp.watch(paths.viewsDev, gulp.series(views, js));
                gulp.watch(paths.lang, gulp.series(lang));
                gulp.watch(paths.manifest, gulp.series(manifest));

                done();
            }
        )));
