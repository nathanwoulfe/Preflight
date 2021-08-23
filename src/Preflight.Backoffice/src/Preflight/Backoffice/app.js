import { ServicesModule } from './resources/_servicesModule';
import { ComponentsModule } from './components/_componentsModule';
import { ControllersModule } from './controllers/_controllersModule';
import { PluginsModule } from './plugins/_pluginsModule';

const name = 'preflight';

angular.module(name, [
    ServicesModule,
    ComponentsModule,
    ControllersModule,
    PluginsModule,
]);

angular.module('umbraco').requires.push(name);