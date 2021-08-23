import gulp from 'gulp';
import gulpif from 'gulp-if';

import { paths, config } from './config';

// in dev, copy components as the html is not inlined
export function views() {
    const backoffice = 'Backoffice/';
    return gulp.src(config.prod ? paths.viewsProd : paths.viewsDev)
        .pipe(gulpif(!config.prod, gulp.dest(paths.siteNetCore + backoffice)))
        .pipe(gulpif(!config.prod, gulp.dest(paths.site + backoffice)))
        .pipe(gulpif(config.prod, gulp.dest(paths.dest + backoffice)));
}