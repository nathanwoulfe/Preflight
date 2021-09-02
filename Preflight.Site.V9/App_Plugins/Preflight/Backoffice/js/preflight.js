(function(){function r(e,n,t){function o(i,f){if(!n[i]){if(!e[i]){var c="function"==typeof require&&require;if(!f&&c)return c(i,!0);if(u)return u(i,!0);var a=new Error("Cannot find module '"+i+"'");throw a.code="MODULE_NOT_FOUND",a}var p=n[i]={exports:{}};e[i][0].call(p.exports,function(r){var n=e[i][1][r];return o(n||r)},p,p.exports,r,e,n,t)}return n[i].exports}for(var u="function"==typeof require&&require,i=0;i<t.length;i++)o(t[i]);return o}return r})()({1:[function(require,module,exports){
"use strict";

var _servicesModule = require("./resources/_servicesModule");

var _componentsModule = require("./components/_componentsModule");

var _controllersModule = require("./controllers/_controllersModule");

var _pluginsModule = require("./plugins/_pluginsModule");

var name = 'preflight';
angular.module(name, [_servicesModule.ServicesModule, _componentsModule.ComponentsModule, _controllersModule.ControllersModule, _pluginsModule.PluginsModule]);
angular.module('umbraco').requires.push(name);

},{"./components/_componentsModule":2,"./controllers/_controllersModule":8,"./plugins/_pluginsModule":13,"./resources/_servicesModule":16}],2:[function(require,module,exports){
"use strict";

Object.defineProperty(exports, "__esModule", {
  value: true
});
exports.ComponentsModule = void 0;

var _card = require("./card/card.component");

var _progresscircle = require("./progresscircle/progresscircle.component");

var _resultintro = require("./resultintro.component");

var _stateicon = require("./stateicon/stateicon.component");

var ComponentsModule = angular.module('preflight.components', []).component(_card.CardComponent.name, _card.CardComponent).component(_resultintro.ResultIntroComponent.name, _resultintro.ResultIntroComponent).component(_stateicon.StateIconComponent.name, _stateicon.StateIconComponent).component(_progresscircle.ProgressCircleComponent.name, _progresscircle.ProgressCircleComponent).name;
exports.ComponentsModule = ComponentsModule;

},{"./card/card.component":3,"./progresscircle/progresscircle.component":4,"./resultintro.component":5,"./stateicon/stateicon.component":6}],3:[function(require,module,exports){
"use strict";

Object.defineProperty(exports, "__esModule", {
  value: true
});
exports.CardComponent = void 0;

function _classCallCheck(instance, Constructor) { if (!(instance instanceof Constructor)) { throw new TypeError("Cannot call a class as a function"); } }

function _defineProperties(target, props) { for (var i = 0; i < props.length; i++) { var descriptor = props[i]; descriptor.enumerable = descriptor.enumerable || false; descriptor.configurable = true; if ("value" in descriptor) descriptor.writable = true; Object.defineProperty(target, descriptor.key, descriptor); } }

function _createClass(Constructor, protoProps, staticProps) { if (protoProps) _defineProperties(Constructor.prototype, protoProps); if (staticProps) _defineProperties(Constructor, staticProps); return Constructor; }

function _defineProperty(obj, key, value) { if (key in obj) { Object.defineProperty(obj, key, { value: value, enumerable: true, configurable: true, writable: true }); } else { obj[key] = value; } return obj; }

var Card = /*#__PURE__*/function () {
  function Card(localizationService) {
    _classCallCheck(this, Card);

    _defineProperty(this, "cardClass", 'pass');

    _defineProperty(this, "cardScoreClass", 'pass-color');

    _defineProperty(this, "localizationService", void 0);

    this.localizationService = localizationService;
  }

  _createClass(Card, [{
    key: "$onInit",
    value: function $onInit() {
      var _this = this;

      if (this.failed) {
        this.cardClass = 'fail';
        this.cardScoreClass = 'fail-color';
      }

      if (this.title[0] === '@') {
        this.localizationService.localize(this.title, this.tokens).then(function (localizedTitle) {
          return _this.title = localizedTitle;
        });
      }

      if (this.subtitle[0] === '@') {
        this.localizationService.localize(this.subtitle, this.tokens).then(function (localizedSubtitle) {
          return _this.subtitle = localizedSubtitle;
        });
      }
    }
  }]);

  return Card;
}();

_defineProperty(Card, "template", "\n        <div class=\"card {{ ::$ctrl.cardClass }}\">\n            <span class=\"card-score {{ ::$ctrl.cardScoreClass }}\" ng-bind=\"::$ctrl.score\"></span>\n            <span class=\"card-title\">\n                {{ ::$ctrl.title }}<br />\n                {{ ::$ctrl.subtitle }}\n            </span>\n        </div>");

var CardComponent = {
  transclude: true,
  name: 'preflightCard',
  bindings: {
    title: '@?',
    subtitle: '@?',
    failed: '<',
    score: '<',
    tokens: '<'
  },
  template: Card.template,
  controller: Card
};
exports.CardComponent = CardComponent;

},{}],4:[function(require,module,exports){
"use strict";

Object.defineProperty(exports, "__esModule", {
  value: true
});
exports.ProgressCircleComponent = void 0;

function _classCallCheck(instance, Constructor) { if (!(instance instanceof Constructor)) { throw new TypeError("Cannot call a class as a function"); } }

function _defineProperty(obj, key, value) { if (key in obj) { Object.defineProperty(obj, key, { value: value, enumerable: true, configurable: true, writable: true }); } else { obj[key] = value; } return obj; }

var ProgressCircle = function ProgressCircle($element) {
  var _this = this;

  _classCallCheck(this, ProgressCircle);

  _defineProperty(this, "r", void 0);

  _defineProperty(this, "$element", void 0);

  _defineProperty(this, "$onChanges", function () {
    _this.draw();
  });

  _defineProperty(this, "draw", function () {
    var percent = Math.round(_this.percentage);
    percent = percent > 100 ? 100 : percent || 0; // calculating the circle's highlight

    var pathLength = _this.r * Math.PI * 2 * percent / 100; // Full circle length

    _this.dashArray = "".concat(pathLength, ",100"); // set font size

    _this.percentageSize = _this.size * 0.3 + 'px'; // use rounded percentage

    _this.label = "".concat(percent, "%");
  });

  this.r = $element.find('.umb-progress-circle__highlight').attr('r');
};

_defineProperty(ProgressCircle, "template", "\n        <div class=\"umb-progress-circle preflight-progress-circle\" ng-style=\"{'width': $ctrl.size, 'height': $ctrl.size }\">\n            <svg class=\"umb-progress-circle__view-box\" viewBox=\"0 0 33.83098862 33.83098862\"> \n                <circle class=\"umb-progress-circle__highlight--{{ $ctrl.background }}\" cx=\"16.91549431\" cy=\"16.91549431\" r=\"15.91549431\" fill=\"none\" stroke-width=\".5\"></circle>\n                <circle class=\"umb-progress-circle__highlight umb-progress-circle__highlight--{{ $ctrl.foreground }}\"\n                    cx=\"16.91549431\" cy=\"16.91549431\" r=\"15.91549431\" stroke-linecap=\"round\" fill=\"none\" stroke-width=\"2\" stroke-dasharray=\"{{ $ctrl.dashArray }}\"></circle>\n            </svg> \n            <div ng-style=\"{'font-size': $ctrl.percentageSize}\" class=\"umb-progress-circle__percentage\">\n                {{ $ctrl.label }}\n                <small>pass rate</small>                \n            </div>\n        </div>");

var ProgressCircleComponent = {
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
exports.ProgressCircleComponent = ProgressCircleComponent;

},{}],5:[function(require,module,exports){
"use strict";

Object.defineProperty(exports, "__esModule", {
  value: true
});
exports.ResultIntroComponent = void 0;

function _classCallCheck(instance, Constructor) { if (!(instance instanceof Constructor)) { throw new TypeError("Cannot call a class as a function"); } }

function _defineProperties(target, props) { for (var i = 0; i < props.length; i++) { var descriptor = props[i]; descriptor.enumerable = descriptor.enumerable || false; descriptor.configurable = true; if ("value" in descriptor) descriptor.writable = true; Object.defineProperty(target, descriptor.key, descriptor); } }

function _createClass(Constructor, protoProps, staticProps) { if (protoProps) _defineProperties(Constructor.prototype, protoProps); if (staticProps) _defineProperties(Constructor, staticProps); return Constructor; }

function _defineProperty(obj, key, value) { if (key in obj) { Object.defineProperty(obj, key, { value: value, enumerable: true, configurable: true, writable: true }); } else { obj[key] = value; } return obj; }

var ResultIntro = /*#__PURE__*/function () {
  /**
   * Directive used to render the heading for a plugin in the results view
   * Send a stringified array as the tokens attribute to replace %0%, %1% .. %n% in the localized string
   */
  function ResultIntro(localizationService) {
    _classCallCheck(this, ResultIntro);

    _defineProperty(this, "localizationService", void 0);

    this.localizationService = localizationService;
  }

  _createClass(ResultIntro, [{
    key: "$onInit",
    value: function $onInit() {
      var _this = this;

      if (this.passText[0] === '@') {
        this.localizationService.localize(this.passText, this.tokens).then(function (localizedPassText) {
          return _this.passText = localizedPassText;
        });
      }

      if (this.heading[0] === '@') {
        this.localizationService.localize(this.heading, this.tokens).then(function (localizedHeading) {
          return _this.heading = localizedHeading;
        });
      }
    }
  }]);

  return ResultIntro;
}();

_defineProperty(ResultIntro, "template", "\n        <h5 ng-bind=\"::$ctrl.heading\" class=\"mt0\"></h5>\n        <span ng-if=\"$ctrl.pass\" ng-bind=\"::$ctrl.passText\"></span>");

var ResultIntroComponent = {
  transclude: true,
  name: 'preflightResultIntro',
  bindings: {
    tokens: '<',
    passText: '@?',
    heading: '@?',
    pass: '<'
  },
  template: ResultIntro.template,
  controller: ResultIntro
};
exports.ResultIntroComponent = ResultIntroComponent;

},{}],6:[function(require,module,exports){
"use strict";

Object.defineProperty(exports, "__esModule", {
  value: true
});
exports.StateIconComponent = void 0;

function _classCallCheck(instance, Constructor) { if (!(instance instanceof Constructor)) { throw new TypeError("Cannot call a class as a function"); } }

function _defineProperties(target, props) { for (var i = 0; i < props.length; i++) { var descriptor = props[i]; descriptor.enumerable = descriptor.enumerable || false; descriptor.configurable = true; if ("value" in descriptor) descriptor.writable = true; Object.defineProperty(target, descriptor.key, descriptor); } }

function _createClass(Constructor, protoProps, staticProps) { if (protoProps) _defineProperties(Constructor.prototype, protoProps); if (staticProps) _defineProperties(Constructor, staticProps); return Constructor; }

function _defineProperty(obj, key, value) { if (key in obj) { Object.defineProperty(obj, key, { value: value, enumerable: true, configurable: true, writable: true }); } else { obj[key] = value; } return obj; }

var StateIcon = /*#__PURE__*/function () {
  function StateIcon() {
    _classCallCheck(this, StateIcon);

    _defineProperty(this, "icon", 'power');

    _defineProperty(this, "className", 'disabled');
  }

  _createClass(StateIcon, [{
    key: "$onInit",
    value: function $onInit() {
      if (!this.disabled) {
        this.icon = this.failed ? 'delete' : 'check';
        this.className = this.failed ? 'fail' : 'pass';
      }
    }
  }]);

  return StateIcon;
}();

_defineProperty(StateIcon, "template", "\n        <div class=\"state-icon {{ ::$ctrl.className }}\">\n            <umb-icon icon=\"icon-{{ ::$ctrl.icon }}\"></umb-icon>\n        </div>");

var StateIconComponent = {
  transclude: true,
  name: 'preflightStateIcon',
  bindings: {
    failed: '<',
    disabled: '<'
  },
  template: StateIcon.template,
  controller: StateIcon
};
exports.StateIconComponent = StateIconComponent;

},{}],7:[function(require,module,exports){
"use strict";

Object.defineProperty(exports, "__esModule", {
  value: true
});
exports.constants = void 0;
var constants = {
  checkboxlist: 'checkboxlist',
  multipletextbox: 'multipletextbox'
};
exports.constants = constants;

},{}],8:[function(require,module,exports){
"use strict";

Object.defineProperty(exports, "__esModule", {
  value: true
});
exports.ControllersModule = void 0;

var _preflight = require("./preflight.controller");

var _notification = require("./notification.controller");

var _settings = require("./settings.controller");

var ControllersModule = angular.module('preflight.controllers', []).controller(_preflight.PreflightController.controllerName, _preflight.PreflightController).controller(_notification.NotificationController.controllerName, _notification.NotificationController).controller(_settings.SettingsController.controllerName, _settings.SettingsController).name;
exports.ControllersModule = ControllersModule;

},{"./notification.controller":9,"./preflight.controller":10,"./settings.controller":11}],9:[function(require,module,exports){
"use strict";

Object.defineProperty(exports, "__esModule", {
  value: true
});
exports.NotificationController = void 0;

function _classCallCheck(instance, Constructor) { if (!(instance instanceof Constructor)) { throw new TypeError("Cannot call a class as a function"); } }

function _defineProperty(obj, key, value) { if (key in obj) { Object.defineProperty(obj, key, { value: value, enumerable: true, configurable: true, writable: true }); } else { obj[key] = value; } return obj; }

var NotificationController = function NotificationController() {
  _classCallCheck(this, NotificationController);

  _defineProperty(this, "saveCancelled", void 0);
};

exports.NotificationController = NotificationController;

_defineProperty(NotificationController, "controllerName", 'preflight.notification.controller');

},{}],10:[function(require,module,exports){
"use strict";

Object.defineProperty(exports, "__esModule", {
  value: true
});
exports.PreflightController = void 0;

function _extends() { _extends = Object.assign || function (target) { for (var i = 1; i < arguments.length; i++) { var source = arguments[i]; for (var key in source) { if (Object.prototype.hasOwnProperty.call(source, key)) { target[key] = source[key]; } } } return target; }; return _extends.apply(this, arguments); }

function _createForOfIteratorHelper(o, allowArrayLike) { var it = typeof Symbol !== "undefined" && o[Symbol.iterator] || o["@@iterator"]; if (!it) { if (Array.isArray(o) || (it = _unsupportedIterableToArray(o)) || allowArrayLike && o && typeof o.length === "number") { if (it) o = it; var i = 0; var F = function F() {}; return { s: F, n: function n() { if (i >= o.length) return { done: true }; return { done: false, value: o[i++] }; }, e: function e(_e) { throw _e; }, f: F }; } throw new TypeError("Invalid attempt to iterate non-iterable instance.\nIn order to be iterable, non-array objects must have a [Symbol.iterator]() method."); } var normalCompletion = true, didErr = false, err; return { s: function s() { it = it.call(o); }, n: function n() { var step = it.next(); normalCompletion = step.done; return step; }, e: function e(_e2) { didErr = true; err = _e2; }, f: function f() { try { if (!normalCompletion && it["return"] != null) it["return"](); } finally { if (didErr) throw err; } } }; }

function _unsupportedIterableToArray(o, minLen) { if (!o) return; if (typeof o === "string") return _arrayLikeToArray(o, minLen); var n = Object.prototype.toString.call(o).slice(8, -1); if (n === "Object" && o.constructor) n = o.constructor.name; if (n === "Map" || n === "Set") return Array.from(o); if (n === "Arguments" || /^(?:Ui|I)nt(?:8|16|32)(?:Clamped)?Array$/.test(n)) return _arrayLikeToArray(o, minLen); }

function _arrayLikeToArray(arr, len) { if (len == null || len > arr.length) len = arr.length; for (var i = 0, arr2 = new Array(len); i < len; i++) { arr2[i] = arr[i]; } return arr2; }

function _classCallCheck(instance, Constructor) { if (!(instance instanceof Constructor)) { throw new TypeError("Cannot call a class as a function"); } }

function _defineProperties(target, props) { for (var i = 0; i < props.length; i++) { var descriptor = props[i]; descriptor.enumerable = descriptor.enumerable || false; descriptor.configurable = true; if ("value" in descriptor) descriptor.writable = true; Object.defineProperty(target, descriptor.key, descriptor); } }

function _createClass(Constructor, protoProps, staticProps) { if (protoProps) _defineProperties(Constructor.prototype, protoProps); if (staticProps) _defineProperties(Constructor, staticProps); return Constructor; }

function _defineProperty(obj, key, value) { if (key in obj) { Object.defineProperty(obj, key, { value: value, enumerable: true, configurable: true, writable: true }); } else { obj[key] = value; } return obj; }

var PreflightController = /*#__PURE__*/function () {
  function PreflightController($scope, $rootScope, $element, $timeout, editorState, preflightService, preflightHub) {
    var _this = this;

    _classCallCheck(this, PreflightController);

    _defineProperty(this, "$scope", void 0);

    _defineProperty(this, "$rootScope", void 0);

    _defineProperty(this, "$element", void 0);

    _defineProperty(this, "$timeout", void 0);

    _defineProperty(this, "editorState", void 0);

    _defineProperty(this, "preflightService", void 0);

    _defineProperty(this, "preflightHub", void 0);

    _defineProperty(this, "dirtyHashes", {});

    _defineProperty(this, "validPropTypes", void 0);

    _defineProperty(this, "propsBeingChecked", []);

    _defineProperty(this, "propertiesToTrack", []);

    _defineProperty(this, "dirtyProps", []);

    _defineProperty(this, "results", {
      properties: []
    });

    _defineProperty(this, "jsonProperties", ['Umbraco.Grid', 'Umbraco.NestedContent']);

    _defineProperty(this, "blockListEditorAlias", 'Umbraco.BlockList');

    _defineProperty(this, "noTests", false);

    _defineProperty(this, "percentageDone", 20);

    _defineProperty(this, "progressStep", 0);

    _defineProperty(this, "activeVariant", void 0);

    _defineProperty(this, "joinList", function (arr) {
      var outStr;

      if (arr.length === 1) {
        outStr = arr[0];
      } else if (arr.length === 2) {
        outStr = arr.join(' and ');
      } else if (arr.length > 2) {
        outStr = arr.slice(0, -1).join(', ') + ', and ' + arr.slice(-1);
      }

      return outStr;
    });

    _defineProperty(this, "getHash", function (s) {
      return s ? s.split('').reduce(function (a, b) {
        a = (a << 5) - a + b.charCodeAt(0);
        return a & a;
      }, 0) : 1;
    });

    _defineProperty(this, "getProperty", function (alias) {
      var _iterator = _createForOfIteratorHelper(_this.editorState.current.variants.find(function (x) {
        return x.active;
      }).tabs),
          _step;

      try {
        for (_iterator.s(); !(_step = _iterator.n()).done;) {
          var tab = _step.value;

          var _iterator2 = _createForOfIteratorHelper(tab.properties),
              _step2;

          try {
            for (_iterator2.s(); !(_step2 = _iterator2.n()).done;) {
              var prop = _step2.value;

              if (prop.alias === alias) {
                return prop;
              }
            }
          } catch (err) {
            _iterator2.e(err);
          } finally {
            _iterator2.f();
          }
        }
      } catch (err) {
        _iterator.e(err);
      } finally {
        _iterator.f();
      }
    });

    _defineProperty(this, "onComplete", function () {
      // it's possible no tests ran, in which case results won't exist
      _this.noTests = _this.results.properties.every(function (x) {
        return !x.plugins.length;
      });

      if (_this.noTests) {
        _this.$scope.model.badge = undefined;
      }

      var _iterator3 = _createForOfIteratorHelper(_this.results.properties),
          _step3;

      try {
        for (_iterator3.s(); !(_step3 = _iterator3.n()).done;) {
          var p = _step3.value;
          p.disabled = p.failedCount === -1;
        }
      } catch (err) {
        _iterator3.e(err);
      } finally {
        _iterator3.f();
      }

      _this.showSuccessMessage = !_this.results.failed && !_this.noTests;
      _this.done = true;
    });

    _defineProperty(this, "setBadgeCount", function (pending) {
      if (pending) {
        _this.$scope.model.badge = {
          type: 'warning'
        };
        return;
      }

      if (_this.results && _this.results.failedCount > 0) {
        _this.$scope.model.badge = {
          count: _this.results.failedCount,
          type: 'alert --error-badge pf-block'
        };
      } else {
        _this.$scope.model.badge = {
          type: 'success icon-'
        };
      }
    });

    _defineProperty(this, "getCurrentCulture", function () {
      return _this.activeVariant.language ? _this.activeVariant.language.culture : '';
    });

    _defineProperty(this, "rebindResult", function (data) {
      console.log(data.label, data);
      var newProp = true;
      var totalTestsRun = 0;

      var existingProp = _this.results.properties.find(function (x) {
        return x.label === data.label;
      });

      if (existingProp) {
        existingProp = _extends(existingProp, data);
        existingProp.loading = false;
        newProp = false;
      } // a new property will have a temporary placeholder - remove it
      // _temp ensures grid with multiple editors only removes the correct temp entry


      if (newProp && !data.remove && data.failedCount !== -1) {
        var tempIndex = _this.results.properties.findIndex(function (p) {
          return p.name === "".concat(data.name, "_temp");
        });

        if (tempIndex !== -1) {
          _this.results.properties.splice(tempIndex, 1);
        }

        _this.results.properties.push(data);
      }

      _this.results.properties = _this.results.properties.filter(function (x) {
        return x.remove === false;
      });
      _this.results.properties = _this.results.properties.filter(function (x) {
        return x.failedCount > -1;
      });
      _this.results.failedCount = _this.results.properties.reduce(function (prev, cur) {
        totalTestsRun += cur.totalTests;
        return prev + cur.failedCount;
      }, 0);
      _this.results.failed = _this.results.failedCount > 0;
      _this.propsBeingCheckedStr = _this.joinList(_this.propsBeingChecked.splice(_this.propsBeingChecked.indexOf(data.name), 1));
      _this.percentageFailed = (totalTestsRun - _this.results.failedCount) / totalTestsRun * 100;
    });

    _defineProperty(this, "checkDirty", function () {
      _this.dirtyProps = [];
      _this.hasDirty = false;

      var _iterator4 = _createForOfIteratorHelper(_this.propertiesToTrack),
          _step4;

      try {
        for (_iterator4.s(); !(_step4 = _iterator4.n()).done;) {
          var prop = _step4.value;

          var currentValue = _this.getProperty(prop.alias).value;

          if (prop.editor === _this.blockListEditorAlias) {
            currentValue = JSON.stringify(currentValue.contentData);
          } else {
            currentValue = _this.jsonProperties.includes(prop.editor) ? JSON.stringify(currentValue) : currentValue;
          }

          var hash = _this.getHash(currentValue);

          if (_this.dirtyHashes[prop.label] && _this.dirtyHashes[prop.label] !== hash) {
            _this.dirtyProps.push({
              name: prop.label,
              value: currentValue,
              editor: prop.editor
            });

            _this.dirtyHashes[prop.label] = hash;
            _this.hasDirty = true;
          } else if (!_this.dirtyHashes[prop.label]) {
            _this.dirtyHashes[prop.label] = hash;
          }
        } // if dirty properties exist, create a simple model for each and send the lot off for checking
        // response comes via the signalr hub so is not handled here

      } catch (err) {
        _iterator4.e(err);
      } finally {
        _iterator4.f();
      }

      if (_this.hasDirty) {
        _this.$timeout(function () {
          _this.dirtyProps.forEach(function (prop) {
            var _iterator5 = _createForOfIteratorHelper(_this.results.properties.filter(function (p) {
              return p.name === prop.name;
            })),
                _step5;

            try {
              for (_iterator5.s(); !(_step5 = _iterator5.n()).done;) {
                var existing = _step5.value;

                if (existing) {
                  existing.open = false;
                  existing.failedCount = -1;
                } else {
                  // generate new placeholder for pending results - this is removed when the response is returned
                  _this.results.properties.push({
                    label: prop.name,
                    open: false,
                    failed: false,
                    failedCount: -1,
                    name: "".concat(prop.name, "_temp")
                  });
                }
              }
            } catch (err) {
              _iterator5.e(err);
            } finally {
              _iterator5.f();
            }

            _this.propsBeingChecked.push(prop.name);
          });

          _this.propsBeingCheckedStr = _this.joinList(_this.propsBeingChecked);
          var payload = {
            properties: _this.dirtyProps,
            culture: _this.getCurrentCulture(),
            id: _this.editorState.current.id
          };

          _this.setBadgeCount(true);

          _this.done = false;

          _this.preflightService.checkDirty(payload);
        });
      }
    });

    _defineProperty(this, "initSignalR", function () {
      _this.preflightHub.initHub(function (hub) {
        hub.on('preflightTest', function (e) {
          _this.rebindResult(e);

          _this.setBadgeCount();
        });
        hub.on('preflightComplete', function () {
          return _this.onComplete();
        });
        hub.start(function () {
          /**
           * Check all properties when the controller loads. Won't re-run when changing between apps
           * but needs to happen after the hub loads
           */
          _this.$timeout(function () {
            _this.setBadgeCount(true);

            _this.checkDirty(); // builds initial hash array, but won't run anything                    


            _this.preflightService.check(_this.editorState.current.id, _this.getCurrentCulture());
          });
        });
      });
    });

    this.$scope = $scope;
    this.$rootScope = $rootScope;
    this.$element = $element;
    this.$timeout = $timeout;
    this.editorState = editorState;
    this.preflightService = preflightService;
    this.preflightHub = preflightHub;
    this.validPropTypes = Umbraco.Sys.ServerVariables.Preflight.PropertyTypesToCheck;
    this.$scope.model.badge = {
      type: 'info'
    };
    $rootScope.$on('app.tabChange', function (e, data) {
      if (data.alias === 'preflight') {
        // collapse open nc controls, timeouts prevent $apply errors
        var _iterator6 = _createForOfIteratorHelper(document.querySelectorAll('.umb-nested-content__item--active .umb-nested-content__header-bar')),
            _step6;

        try {
          var _loop = function _loop() {
            var openNc = _step6.value;
            $timeout(function () {
              return openNc.click();
            });
          };

          for (_iterator6.s(); !(_step6 = _iterator6.n()).done;) {
            _loop();
          }
        } catch (err) {
          _iterator6.e(err);
        } finally {
          _iterator6.f();
        }

        $timeout(function () {
          _this.checkDirty();

          _this.setBadgeCount();
        });
      }
    });
    $rootScope.$on('showPreflight', function (event, data) {
      if (data.nodeId === $scope.content.id) {
        // needs to find app closest to current scope
        var appLink = $element.closest('form').find('[data-element="sub-view-preflight"]');

        if (appLink) {
          appLink.click();
        }
      }
    });
  }
  /**
   * 
   * @param {any} arr
   */


  _createClass(PreflightController, [{
    key: "$onInit",
    value: function $onInit() {
      var _this2 = this;

      this.activeVariant = this.editorState.current.variants.find(function (x) {
        return x.active;
      });
      this.propertiesToTrack = [];

      if (this.activeVariant) {
        this.activeVariant.tabs.forEach(function (x) {
          _this2.propertiesToTrack = _this2.propertiesToTrack.concat(x.properties.map(function (x) {
            if (_this2.validPropTypes.includes(x.editor)) {
              return {
                editor: x.editor,
                alias: x.alias,
                label: x.label
              };
            }
          })).filter(function (x) {
            return x;
          });
        }); // array will have length, as app is only sent on types with testable properties

        if (this.propertiesToTrack.length) {
          this.initSignalR();
        }
      }
    }
  }]);

  return PreflightController;
}();

exports.PreflightController = PreflightController;

_defineProperty(PreflightController, "controllerName", 'preflight.controller');

},{}],11:[function(require,module,exports){
"use strict";

Object.defineProperty(exports, "__esModule", {
  value: true
});
exports.SettingsController = void 0;

var _constants = require("../constants");

function _slicedToArray(arr, i) { return _arrayWithHoles(arr) || _iterableToArrayLimit(arr, i) || _unsupportedIterableToArray(arr, i) || _nonIterableRest(); }

function _nonIterableRest() { throw new TypeError("Invalid attempt to destructure non-iterable instance.\nIn order to be iterable, non-array objects must have a [Symbol.iterator]() method."); }

function _iterableToArrayLimit(arr, i) { var _i = arr == null ? null : typeof Symbol !== "undefined" && arr[Symbol.iterator] || arr["@@iterator"]; if (_i == null) return; var _arr = []; var _n = true; var _d = false; var _s, _e; try { for (_i = _i.call(arr); !(_n = (_s = _i.next()).done); _n = true) { _arr.push(_s.value); if (i && _arr.length === i) break; } } catch (err) { _d = true; _e = err; } finally { try { if (!_n && _i["return"] != null) _i["return"](); } finally { if (_d) throw _e; } } return _arr; }

function _arrayWithHoles(arr) { if (Array.isArray(arr)) return arr; }

function _createForOfIteratorHelper(o, allowArrayLike) { var it = typeof Symbol !== "undefined" && o[Symbol.iterator] || o["@@iterator"]; if (!it) { if (Array.isArray(o) || (it = _unsupportedIterableToArray(o)) || allowArrayLike && o && typeof o.length === "number") { if (it) o = it; var i = 0; var F = function F() {}; return { s: F, n: function n() { if (i >= o.length) return { done: true }; return { done: false, value: o[i++] }; }, e: function e(_e2) { throw _e2; }, f: F }; } throw new TypeError("Invalid attempt to iterate non-iterable instance.\nIn order to be iterable, non-array objects must have a [Symbol.iterator]() method."); } var normalCompletion = true, didErr = false, err; return { s: function s() { it = it.call(o); }, n: function n() { var step = it.next(); normalCompletion = step.done; return step; }, e: function e(_e3) { didErr = true; err = _e3; }, f: function f() { try { if (!normalCompletion && it["return"] != null) it["return"](); } finally { if (didErr) throw err; } } }; }

function _unsupportedIterableToArray(o, minLen) { if (!o) return; if (typeof o === "string") return _arrayLikeToArray(o, minLen); var n = Object.prototype.toString.call(o).slice(8, -1); if (n === "Object" && o.constructor) n = o.constructor.name; if (n === "Map" || n === "Set") return Array.from(o); if (n === "Arguments" || /^(?:Ui|I)nt(?:8|16|32)(?:Clamped)?Array$/.test(n)) return _arrayLikeToArray(o, minLen); }

function _arrayLikeToArray(arr, len) { if (len == null || len > arr.length) len = arr.length; for (var i = 0, arr2 = new Array(len); i < len; i++) { arr2[i] = arr[i]; } return arr2; }

function _classCallCheck(instance, Constructor) { if (!(instance instanceof Constructor)) { throw new TypeError("Cannot call a class as a function"); } }

function _defineProperties(target, props) { for (var i = 0; i < props.length; i++) { var descriptor = props[i]; descriptor.enumerable = descriptor.enumerable || false; descriptor.configurable = true; if ("value" in descriptor) descriptor.writable = true; Object.defineProperty(target, descriptor.key, descriptor); } }

function _createClass(Constructor, protoProps, staticProps) { if (protoProps) _defineProperties(Constructor.prototype, protoProps); if (staticProps) _defineProperties(Constructor, staticProps); return Constructor; }

function _defineProperty(obj, key, value) { if (key in obj) { Object.defineProperty(obj, key, { value: value, enumerable: true, configurable: true, writable: true }); } else { obj[key] = value; } return obj; }

var SettingsController = /*#__PURE__*/function () {
  function SettingsController($scope, $q, notificationsService, languageResource, localizationService, preflightService) {
    var _this = this;

    _classCallCheck(this, SettingsController);

    _defineProperty(this, "$scope", void 0);

    _defineProperty(this, "$q", void 0);

    _defineProperty(this, "languageResource", void 0);

    _defineProperty(this, "localizationService", void 0);

    _defineProperty(this, "notificationsService", void 0);

    _defineProperty(this, "preflightService", void 0);

    _defineProperty(this, "languages", []);

    _defineProperty(this, "tabs", []);

    _defineProperty(this, "settings", []);

    _defineProperty(this, "currentLanguage", void 0);

    _defineProperty(this, "languageChangeWatcher", void 0);

    _defineProperty(this, "testablePropertiesWatcher", void 0);

    _defineProperty(this, "watchTestableProperties", function () {
      _this.testablePropertiesWatcher = _this.$scope.$watch(function () {
        return _this.settings.find(function (x) {
          return x.alias === 'propertiesToTest';
        }).value;
      }, function (newVal) {
        if (newVal) {
          var propertiesToModify = _this.settings.filter(function (x) {
            return x.alias.includes('PropertiesToTest') && x.alias !== 'propertiesToTest';
          });

          var _iterator = _createForOfIteratorHelper(propertiesToModify),
              _step;

          try {
            for (_iterator.s(); !(_step = _iterator.n()).done;) {
              var prop = _step.value;

              // use the prop alias to find the checkbox set
              var _iterator2 = _createForOfIteratorHelper(document.querySelectorAll("umb-checkbox[name*=\"".concat(prop.alias, "\"]"))),
                  _step2;

              try {
                for (_iterator2.s(); !(_step2 = _iterator2.n()).done;) {
                  var checkbox = _step2.value;
                  checkbox.querySelector('.umb-form-check').classList[newVal.indexOf(checkbox.getAttribute('value')) === -1 ? 'add' : 'remove']('pf-disabled');
                }
              } catch (err) {
                _iterator2.e(err);
              } finally {
                _iterator2.f();
              }
            }
          } catch (err) {
            _iterator.e(err);
          } finally {
            _iterator.f();
          }
        }
      }, true);
    });

    _defineProperty(this, "saveSettings", function () {
      // ensure the current language is correctly mapped to the sync model
      _this.settings.forEach(function (s) {
        var syncSetting = _this.settingsSyncModel.find(function (x) {
          return x.alias === s.alias;
        });

        if (!syncSetting.value) {
          syncSetting.value = {};
        }

        syncSetting.value[_this.currentLanguage] = s.value ? s.value : s.view.includes(_constants.constants.checkboxlist) || s.view.includes(_constants.constants.multipletextbox) ? [] : null;
      }); // ensure readability is valid


      var validRange = true;

      _this.languages.forEach(function (l) {
        var culture = l.culture;
        var min = parseInt(_this.settingsSyncModel.find(function (x) {
          return x.alias === 'readabilityTargetMinimum';
        }).value[culture]);
        var max = parseInt(_this.settingsSyncModel.find(function (x) {
          return x.alias === 'readabilityTargetMaximum';
        }).value[culture]);

        if (min > max) {
          _this.notificationsService.error('ERROR', "Unable to save settings - readability minimum cannot be greater than readability maximum (".concat(l.name, ")"));

          validRange = false;
        } else if (min + 10 > max) {
          _this.notificationsService.warning('WARNING', "Readability range is narrow (".concat(l.name, ")"));
        }
      });

      if (validRange) {
        // need to transform multitextbox values without digest
        // so must be a new object, not a reference
        var settingsToSave = JSON.parse(JSON.stringify(_this.settingsSyncModel));
        settingsToSave.forEach(function (v) {
          if (v.view.includes(_constants.constants.multipletextbox)) {
            for (var _i = 0, _Object$entries = Object.entries(v.value); _i < _Object$entries.length; _i++) {
              var _Object$entries$_i = _slicedToArray(_Object$entries[_i], 2),
                  key = _Object$entries$_i[0],
                  value = _Object$entries$_i[1];

              v.value[key] = value.map(function (o) {
                return o.value;
              }).join(',');
            }
          } else if (v.view.includes(_constants.constants.checkboxlist)) {
            for (var _i2 = 0, _Object$entries2 = Object.entries(v.value); _i2 < _Object$entries2.length; _i2++) {
              var _Object$entries2$_i = _slicedToArray(_Object$entries2[_i2], 2),
                  _key = _Object$entries2$_i[0],
                  _value = _Object$entries2$_i[1];

              v.value[_key] = _value.join(',');
            }
          }
        });

        _this.preflightService.saveSettings(settingsToSave, _this.tabs).then(function (_) {
          return _this.$scope.preflightSettingsForm.$setPristine();
        });
      }
    });

    this.$scope = $scope;
    this.$q = $q;
    this.notificationsService = notificationsService;
    this.preflightService = preflightService;
    this.languageResource = languageResource;
    this.localizationService = localizationService;
    this.languageChangeWatcher = $scope.$watch(function () {
      return _this.currentLanguage;
    }, function (newLang, oldLang) {
      // update settings to only include the current variant
      if (newLang && newLang !== oldLang) {
        _this.settings.forEach(function (s) {
          var syncSetting = _this.settingsSyncModel.find(function (x) {
            return x.alias === s.alias;
          }); // manage old language by updating the sync settings model,
          // ensuring the value is an object


          if (oldLang) {
            if (!syncSetting.value) {
              syncSetting.value = {};
            }

            syncSetting.value[oldLang] = s.value;
          } // get the value for the new language and update the settings model


          if (syncSetting.value && syncSetting.value[newLang]) {
            s.value = syncSetting.value[newLang];
          } else {
            // set the value to a sensible default - array if type is checkboxlist
            s.value = s.view.includes(_constants.constants.checkboxlist) || s.view.includes(_constants.constants.multipletextbox) ? [] : null;
          }
        });
      }
    });
  }

  _createClass(SettingsController, [{
    key: "$onDestroy",
    value: function $onDestroy() {
      this.languageChangeWatcher();
      this.testablePropertiesWatcher();
    }
  }, {
    key: "$onInit",
    value: function $onInit() {
      var _this2 = this;

      var promises = [this.preflightService.getSettings(), this.languageResource.getAll()];
      this.$q.all(promises).then(function (resp) {
        _this2.settingsSyncModel = resp[0].data.settings;
        _this2.settings = JSON.parse(JSON.stringify(_this2.settingsSyncModel));
        _this2.tabs = resp[0].data.tabs;
        _this2.languages = resp[1];

        var currentLanguage = _this2.languages.find(function (x) {
          return x.isDefault;
        }).culture;

        _this2.settingsSyncModel.forEach(function (v) {
          if (v.view.includes(_constants.constants.multipletextbox) && v.value) {
            for (var _i3 = 0, _Object$entries3 = Object.entries(v.value); _i3 < _Object$entries3.length; _i3++) {
              var _Object$entries3$_i = _slicedToArray(_Object$entries3[_i3], 2),
                  key = _Object$entries3$_i[0],
                  value = _Object$entries3$_i[1];

              v.value[key] = value.split(',').map(function (val) {
                return {
                  value: val
                };
              }).sort(function (a, b) {
                return a < b;
              });
            }
          } else if (v.view.includes(_constants.constants.checkboxlist) && v.value) {
            for (var _i4 = 0, _Object$entries4 = Object.entries(v.value); _i4 < _Object$entries4.length; _i4++) {
              var _Object$entries4$_i = _slicedToArray(_Object$entries4[_i4], 2),
                  _key2 = _Object$entries4$_i[0],
                  _value2 = _Object$entries4$_i[1];

              v.value[_key2] = _value2.split(',');
            }
          }
        });

        _this2.settings.forEach(function (v) {
          var syncSetting = _this2.settingsSyncModel.find(function (x) {
            return x.alias === v.alias;
          });

          v.value = syncSetting.value ? syncSetting.value[currentLanguage] : null;

          if (v.view.includes('slider')) {
            v.config = {
              handle: 'round',
              initVal1: v.alias === 'longWordSyllables' ? 5 : 65,
              maxVal: v.alias === 'longWordSyllables' ? 10 : 100,
              minVal: 0,
              orientation: 'horizontal',
              step: 1,
              tooltip: 'always',
              tooltipPosition: 'bottom'
            };
          } else if (v.view.includes(_constants.constants.multipletextbox)) {
            v.config = {
              min: 0,
              max: 0
            };
            v.validation = {};
          } else if (v.view.includes(_constants.constants.checkboxlist)) {
            v.config = {
              items: v.prevalues
            };
          }
        });

        _this2.currentLanguage = currentLanguage;

        _this2.watchTestableProperties();
      });
    }
    /**
     * 
     */

  }]);

  return SettingsController;
}();

exports.SettingsController = SettingsController;

_defineProperty(SettingsController, "controllerName", 'preflight.settings.controller');

},{"../constants":7}],12:[function(require,module,exports){
"use strict";

(function () {
  var postSaveUrl = '/umbracoapi/content/postsave';
  var presaveText;

  function interceptor(notificationsService, overlayService, editorState, $rootScope, $q, $injector) {
    var checkGroup = function checkGroup(userGroupOptInOut, culture) {
      var value = userGroupOptInOut.value[culture];
      var enabledGroups = userGroupOptInOut.prevalues.filter(function (x) {
        return value.includes(x.value);
      }).map(function (x) {
        return x.key;
      });
      var localizationService;
      $injector.invoke(['localizationService', function (service) {
        return localizationService = service;
      }]);
      $injector.invoke(['authResource', function (authResource) {
        var promises = [authResource.getCurrentUser(), localizationService.localize('preflight_presaveText')];
        $q.all(promises).then(function (resp) {
          var currentUser = resp[0];
          presaveText = resp[1];

          if (enabledGroups.some(function (x) {
            return currentUser.userGroups.includes(x);
          })) {
            notificationsService.info('Preflight', presaveText);
          }
        });
      }]);
    };

    return {
      request: function request(_request) {
        if (_request.url.toLowerCase().includes(postSaveUrl)) {
          $injector.invoke(['preflightService', function (s) {
            s.getSettings().then(function (resp) {
              var settings = resp.data.settings; // settings values are a dictionary keyed by culture

              var variantsToSave = _request.data.value.variants.filter(function (v) {
                return v.save;
              });

              var preflightVars = Umbraco.Sys.ServerVariables.Preflight;
              variantsToSave.forEach(function (variant) {
                var culture = variant.language ? variant.language.culture : preflightVars.DefaultCulture;
                var runOnSave = settings.find(function (x) {
                  return x.guid === preflightVars.SettingsGuid.BindSaveHandler;
                });

                if (runOnSave && runOnSave.value[culture] === '1') {
                  var userGroupOptInOut = settings.find(function (x) {
                    return x.guid === preflightVars.SettingsGuid.UserGroupOptIn;
                  });
                  checkGroup(userGroupOptInOut, culture);
                }
              });
            });
          }]);
        }

        return _request || $q.when(_request);
      },
      response: function response(_response) {
        try {
          if (_response.config.url.toLowerCase().includes(postSaveUrl)) {
            var index = notificationsService.current.findIndex(function (x) {
              return x.message === presaveText;
            });

            if (index > -1) {
              setTimeout(function () {
                return notificationsService.remove(index);
              }, 1500);
            }

            if (_response.data.notifications) {
              var notification = _response.data.notifications.filter(function (f) {
                return f.header === Umbraco.Sys.ServerVariables.Preflight.ContentFailedChecks;
              })[0];

              if (notification) {
                _response.data.notifications = [];
                overlayService.open({
                  view: "".concat(Umbraco.Sys.ServerVariables.Preflight.PluginPath, "/views/failed.notification.html"),
                  submitButtonLabelKey: 'preflight_review',
                  hideHeader: true,
                  saveCancelled: notification.message.includes('_True'),
                  submit: function submit() {
                    $rootScope.$emit('showPreflight', {
                      nodeId: editorState.current.id
                    });
                    overlayService.close();
                  },
                  close: function close() {
                    return overlayService.close();
                  }
                });
              }
            }
          }
        } catch (err) {
          console.log(err.message);
        }

        return _response || $q.when(_response);
      }
    };
  }

  angular.module('preflight').factory('preflight.save.interceptor', ['notificationsService', 'overlayService', 'editorState', '$rootScope', '$q', '$injector', interceptor]).config(["$httpProvider", function ($httpProvider) {
    return $httpProvider.interceptors.push('preflight.save.interceptor');
  }]);
})();

},{}],13:[function(require,module,exports){
"use strict";

Object.defineProperty(exports, "__esModule", {
  value: true
});
exports.PluginsModule = void 0;

var _linkhealthResult = require("./linkhealth/linkhealth.result.component");

var _readabilityPlugin = require("./readability/readability.plugin.controller");

var PluginsModule = angular.module('preflight.plugins', []).component(_linkhealthResult.LinkHealthResultComponent.name, _linkhealthResult.LinkHealthResultComponent).controller(_readabilityPlugin.ReadabilityPluginController.controllerName, _readabilityPlugin.ReadabilityPluginController).name;
exports.PluginsModule = PluginsModule;

},{"./linkhealth/linkhealth.result.component":14,"./readability/readability.plugin.controller":15}],14:[function(require,module,exports){
"use strict";

Object.defineProperty(exports, "__esModule", {
  value: true
});
exports.LinkHealthResultComponent = void 0;
var LinkHealthResultComponent = {
  name: 'linkHealthResult',
  transclude: true,
  bindings: {
    results: '<'
  },
  template: "\n        <table class=\"linkhealth-result-table\">\n            <thead>\n                <tr><th>Link text</th> <th>Link target</th> <th>Link status</th></tr>\n            </thead>\n            <tr ng-repeat=\"link in $ctrl.results\"><td ng-bind=\"link.text\"></td><td ng-bind=\"link.href\"></td><td ng-bind=\"link.status\"></td></tr>\n        </table>"
};
exports.LinkHealthResultComponent = LinkHealthResultComponent;

},{}],15:[function(require,module,exports){
"use strict";

Object.defineProperty(exports, "__esModule", {
  value: true
});
exports.ReadabilityPluginController = void 0;

function _classCallCheck(instance, Constructor) { if (!(instance instanceof Constructor)) { throw new TypeError("Cannot call a class as a function"); } }

function _defineProperty(obj, key, value) { if (key in obj) { Object.defineProperty(obj, key, { value: value, enumerable: true, configurable: true, writable: true }); } else { obj[key] = value; } return obj; }

var ReadabilityPluginController = function ReadabilityPluginController($scope, overlayService) {
  var _this = this;

  _classCallCheck(this, ReadabilityPluginController);

  _defineProperty(this, "$scope", void 0);

  _defineProperty(this, "overlayService", void 0);

  _defineProperty(this, "help", function () {
    var overlay = {
      view: "".concat(Umbraco.Sys.ServerVariables.Preflight.PluginPath, "/plugins/readability/readability.overlay.html"),
      title: _this.$scope.model.name,
      size: 'medium',
      content: _this.$scope.model.description,
      close: function close() {
        return _this.overlayService.close();
      }
    };

    _this.overlayService.open(overlay);
  });

  this.$scope = $scope;
  this.overlayService = overlayService;
};

exports.ReadabilityPluginController = ReadabilityPluginController;

_defineProperty(ReadabilityPluginController, "controllerName", 'readability.plugin.controller');

},{}],16:[function(require,module,exports){
"use strict";

Object.defineProperty(exports, "__esModule", {
  value: true
});
exports.ServicesModule = void 0;

var _preflight = require("./preflight.service");

var _preflight2 = require("./preflight.hub");

var ServicesModule = angular.module('preflight.services', []).service(_preflight.PreflightService.serviceName, _preflight.PreflightService).service(_preflight2.PreflightHub.serviceName, _preflight2.PreflightHub).name;
exports.ServicesModule = ServicesModule;

},{"./preflight.hub":17,"./preflight.service":18}],17:[function(require,module,exports){
"use strict";

Object.defineProperty(exports, "__esModule", {
  value: true
});
exports.PreflightHub = void 0;

function _classCallCheck(instance, Constructor) { if (!(instance instanceof Constructor)) { throw new TypeError("Cannot call a class as a function"); } }

function _defineProperties(target, props) { for (var i = 0; i < props.length; i++) { var descriptor = props[i]; descriptor.enumerable = descriptor.enumerable || false; descriptor.configurable = true; if ("value" in descriptor) descriptor.writable = true; Object.defineProperty(target, descriptor.key, descriptor); } }

function _createClass(Constructor, protoProps, staticProps) { if (protoProps) _defineProperties(Constructor.prototype, protoProps); if (staticProps) _defineProperties(Constructor, staticProps); return Constructor; }

function _defineProperty(obj, key, value) { if (key in obj) { Object.defineProperty(obj, key, { value: value, enumerable: true, configurable: true, writable: true }); } else { obj[key] = value; } return obj; }

var PreflightHub = /*#__PURE__*/function () {
  function PreflightHub($rootScope, $q, assetsService) {
    var _this = this;

    _classCallCheck(this, PreflightHub);

    _defineProperty(this, "$rootScope", void 0);

    _defineProperty(this, "$q", void 0);

    _defineProperty(this, "assetsService", void 0);

    _defineProperty(this, "scripts", []);

    _defineProperty(this, "callbacks", []);

    _defineProperty(this, "starting", false);

    _defineProperty(this, "platform", Umbraco.Sys.ServerVariables.Preflight.Platform);

    _defineProperty(this, "setupHub", function (callback) {
      var proxy;
      var hub = {};

      if (_this.platform === 'CORE') {
        $.connection = new signalR.HubConnectionBuilder().withUrl(Umbraco.Sys.ServerVariables.Preflight.signalRHub).withAutomaticReconnect().configureLogging(signalR.LogLevel.Warning).build();
        proxy = $.connection;
      } else {
        proxy = $.connection.preflightHub;
      }

      if (proxy !== undefined) {
        hub = {
          active: true,
          start: function start(callback) {
            if (_this.platform === 'CORE') {
              try {
                proxy.start().then(function () {
                  return callback ? callback() : {};
                })["catch"](function () {
                  return console.warn('Failed to start hub');
                });
              } catch (e) {
                console.warn('Could not setup signalR connection', e);
              }
            } else {
              if ($.connection.hub.state !== $.connection.connectionState.disconnected) {
                $.connection.hub.stop(true, true);
              }

              $.connection.hub.start();
              callback ? callback() : {};
            }
          },
          on: function on(eventName, callback) {
            proxy.on(eventName, function (result) {
              _this.$rootScope.$apply(function () {
                if (callback) {
                  callback(result);
                }
              });
            });
          }
        };
      } else {
        hub = {
          on: function on() {},
          start: function start() {
            return console.warn('No hub to start');
          }
        };
      }

      return callback(hub);
    });

    this.$rootScope = $rootScope;
    this.$q = $q;
    this.assetsService = assetsService;
    var umbracoPath = Umbraco.Sys.ServerVariables.umbracoSettings.umbracoPath;

    if (this.platform === 'CORE') {
      this.scripts = [umbracoPath + '/lib/signalr/signalr.min.js'];
    } else {
      this.scripts = [umbracoPath + '/lib/signalr/jquery.signalr.js', umbracoPath + '/backoffice/signalr/hubs'];
    }
  }

  _createClass(PreflightHub, [{
    key: "processCallbacks",
    value:
    /**
     * Function is common across 472 and 5.0
     * */
    function processCallbacks() {
      while (this.callbacks.length) {
        var cb = this.callbacks.pop();
        this.setupHub(cb);
      }

      this.starting = false;
    }
    /**
     * Function is common across 472 and 5.0 
     * @param callback
     */

  }, {
    key: "initHub",
    value: function initHub(callback) {
      var _this2 = this;

      this.callbacks.push(callback);

      if (!this.starting) {
        if ($.connection === undefined) {
          this.starting = true;
          var promises = [];
          this.scripts.forEach(function (script) {
            return promises.push(_this2.assetsService.loadJs(script));
          });
          this.$q.all(promises).then(function () {
            return _this2.processCallbacks();
          });
        } else {
          this.processCallbacks();
        }
      }
    }
  }]);

  return PreflightHub;
}();

exports.PreflightHub = PreflightHub;

_defineProperty(PreflightHub, "serviceName", 'preflightHub');

},{}],18:[function(require,module,exports){
"use strict";

Object.defineProperty(exports, "__esModule", {
  value: true
});
exports.PreflightService = void 0;

function _classCallCheck(instance, Constructor) { if (!(instance instanceof Constructor)) { throw new TypeError("Cannot call a class as a function"); } }

function _defineProperty(obj, key, value) { if (key in obj) { Object.defineProperty(obj, key, { value: value, enumerable: true, configurable: true, writable: true }); } else { obj[key] = value; } return obj; }

var PreflightService = function PreflightService($http, umbRequestHelper) {
  var _this = this;

  _classCallCheck(this, PreflightService);

  _defineProperty(this, "$http", void 0);

  _defineProperty(this, "umbRequestHelper", void 0);

  _defineProperty(this, "urlBase", Umbraco.Sys.ServerVariables.Preflight.ApiPath);

  _defineProperty(this, "request", function (method, url, data) {
    return _this.umbRequestHelper.resourcePromise(method === 'GET' ? _this.$http.get(_this.urlBase + url) : _this.$http.post(_this.urlBase + url, data), 'Something broke');
  });

  _defineProperty(this, "check", function (id, culture) {
    return _this.request('GET', "Check/".concat(id, "/").concat(culture, " "));
  });

  _defineProperty(this, "checkDirty", function (data) {
    return _this.request('POST', "CheckDirty", data);
  });

  _defineProperty(this, "getSettings", function () {
    return _this.request('GET', "GetSettings");
  });

  _defineProperty(this, "saveSettings", function (settings, tabs) {
    return _this.request('POST', "SaveSettings", {
      settings: settings,
      tabs: tabs
    });
  });

  this.$http = $http;
  this.umbRequestHelper = umbRequestHelper;
};

exports.PreflightService = PreflightService;

_defineProperty(PreflightService, "serviceName", 'preflightService');

},{}]},{},[1,7,5,2,9,10,11,8,12,13,17,18,16,3,4,6,14,15])

//# sourceMappingURL=preflight.js.map
