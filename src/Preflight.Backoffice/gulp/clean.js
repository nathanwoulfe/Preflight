import del from 'del';
import { paths, config } from './config';

export function clean() {
    const sitePaths = [`${paths.site}**/*`, `!${paths.site}*.manifest`];
    const siteNetCorePaths = [`${paths.siteNetCore}**/*`, `!${paths.siteNetCore}*.manifest`];

    return del(config.prod
        ? [paths.dest]
        : [...sitePaths, ...siteNetCorePaths], { force: true });
}