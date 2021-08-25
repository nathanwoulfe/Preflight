import { CardComponent } from './card/card.component';
import { ProgressCircleComponent } from './progresscircle/progresscircle.component';
import { ResultIntroComponent } from './resultintro.component';
import { StateIconComponent } from './stateicon/stateicon.component';

export const ComponentsModule = angular
    .module('preflight.components', [])
    .component(CardComponent.name, CardComponent)
    .component(ResultIntroComponent.name, ResultIntroComponent)
    .component(StateIconComponent.name, StateIconComponent)
    .component(ProgressCircleComponent.name, ProgressCircleComponent)
    .name;