import { LinkHealthResultComponent } from './linkhealth.result.component';
import { ReadabilityPluginController } from './readability.plugin.controller';

export const PluginsModule = angular
    .module('preflight.plugins', [])
    .component(LinkHealthResultComponent.name, LinkHealthResultComponent)
    .controller(ReadabilityPluginController.controllerName, ReadabilityPluginController)
    .name;