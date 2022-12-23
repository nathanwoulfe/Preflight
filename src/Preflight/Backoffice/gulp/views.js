import gulp from 'gulp';

import { paths, config } from './config';

// in dev, copy components as the html is not inlined
export function views() {
    return gulp.src(config.prod ? paths.viewsProd : paths.viewsDev)
        .pipe(gulp.dest(paths.dest));
}
