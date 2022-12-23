import gulp from 'gulp';

import { paths, config } from './gulp/config';
import { js } from './gulp/js';
import { scss } from './gulp/scss';
import { views } from './gulp/views';

function lang() {
  const langDest = '/Lang/';

  return gulp.src(paths.lang)
    .pipe(gulp.dest(paths.dest + langDest));
};

// entry points... 
export const prod = gulp.task('build',
  gulp.series(
    done => {
      config.prod = true,
        done();
    },
    gulp.parallel(
      js,
      scss,
      views,
      lang,
    )));

export const dev = gulp.task('dev',
  gulp.parallel(
    js,
    scss,
    views,
    lang,
    done => {
      console.log('watching for changes... ctrl+c to exit');
      gulp.watch(paths.js, gulp.series(js, views));
      gulp.watch(paths.scss, gulp.series(scss, views));
      gulp.watch(paths.viewsDev, gulp.series(views, js));
      gulp.watch(paths.lang, gulp.series(lang));
      done();
    }
  ));
