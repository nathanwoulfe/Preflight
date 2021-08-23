import { CardComponent } from './card.component';
import { ProgressCircleDirective } from './progress.directive';
import { ResultIntroComponent } from './resultintro.component';
import { StateIconComponent } from './stateicon.component';

export const ComponentsModule = angular
    .module('preflight.components', [])
    .component(CardComponent.name, CardComponent)
    .component(ResultIntroComponent.name, ResultIntroComponent)
    .component(StateIconComponent.name, StateIconComponent)
    .directive(ProgressCircleDirective.directiveName, () => new ProgressCircleDirective())
    .name;