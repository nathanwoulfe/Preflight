import gulp from 'gulp';
import sass from 'gulp-dart-sass';
import concat from 'gulp-concat';
import autoprefixer from 'gulp-autoprefixer';
import minifyCss from 'gulp-minify-css';
import gulpif from 'gulp-if';

import { paths, config } from './config';

export function scss() {
  const cssDest = '/Backoffice/css/';

  // filename is .min.css, but only minified for prod
  return gulp.src(paths.scss)
    .pipe(sass())
    .pipe(concat(`preflight.min.css`))
    .pipe(gulpif(config.prod, autoprefixer()))
    .pipe(gulpif(config.prod, minifyCss()))
    .pipe(gulp.dest(paths.dest + cssDest));
}
