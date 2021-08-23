import { PreflightService } from './preflight.service';
import { PreflightHub } from './preflight.hub';

export const ServicesModule = angular
    .module('preflight.services', [])
    .service(PreflightService.serviceName, PreflightService)
    .service(PreflightHub.serviceName, PreflightHub)
    .name;