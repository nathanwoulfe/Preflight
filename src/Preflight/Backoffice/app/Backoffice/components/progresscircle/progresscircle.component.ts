class ProgressCircle {

  static template = `
    <div class="umb-progress-circle preflight-progress-circle" ng-style="{'width': $ctrl.size, 'height': $ctrl.size }">
        <svg class="umb-progress-circle__view-box" viewBox="0 0 33.83098862 33.83098862"> 
            <circle class="umb-progress-circle__highlight--{{ $ctrl.background }}" cx="16.91549431" cy="16.91549431" r="15.91549431" fill="none" stroke-width=".5"></circle>
            <circle class="umb-progress-circle__highlight umb-progress-circle__highlight--{{ $ctrl.foreground }}"
                cx="16.91549431" cy="16.91549431" r="15.91549431" stroke-linecap="round" fill="none" stroke-width="2" stroke-dasharray="{{ $ctrl.dashArray }}"></circle>
        </svg> 
        <div ng-style="{'font-size': $ctrl.percentageSize}" class="umb-progress-circle__percentage">
            {{ $ctrl.label }}
            <small>pass rate</small>                
        </div>
    </div>`;

  radius!: number;
  percentage!: number;
  dashArray!: string;
  size!: number;
  percentageSize!: string;
  label!: string;

  constructor(private $element) {
  }

  $onInit = () => {
    this.radius = this.$element.find('.umb-progress-circle__highlight').attr('r');
  }

  $onChanges = () => {
    this.draw();
  }

  draw = () => {
    let percent = Math.round(this.percentage);
    percent = percent > 100 ? 100 : percent || 0;

    // calculating the circle's highlight
    const pathLength = this.radius * Math.PI * 2 * percent / 100;

    // Full circle length
    this.dashArray = `${pathLength},100`;

    // set font size
    this.percentageSize = this.size * 0.3 + 'px';

    // use rounded percentage
    this.label = `${percent}%`;
  }
}

export const ProgressCircleComponent = {
  transclude: true,
  name: 'progressCircle',
  bindings: {
    size: '@?',
    percentage: '<',
    done: '@',
    foreground: '@',
    background: '@'
  },
  template: ProgressCircle.template,
  controller: ProgressCircle
};
