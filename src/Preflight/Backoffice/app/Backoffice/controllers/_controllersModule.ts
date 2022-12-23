import { PreflightController } from './preflight.controller';
import { NotificationController } from './notification.controller';
import { SettingsController } from './settings.controller';

export const ControllersModule = angular
    .module('preflight.controllers', [])
    .controller(PreflightController.controllerName, PreflightController)
    .controller(NotificationController.controllerName, NotificationController)
    .controller(SettingsController.controllerName, SettingsController)
    .name;