import { ITimeoutService } from "angular";

export class PreflightController {

  static controllerName = 'preflight.controller';

  dirtyHashes = {};
  validPropTypes;
  propsBeingChecked: Array<string> = [];
  propertiesToTrack: Array<IProperty> = [];
  dirtyProps: Array<IProperty> = [];

  done: boolean = false;
  hasDirty: boolean = false;
  showSuccessMessage: boolean = false;

  results: {
    properties: Array<any>,
    failedCount: number,
    failed: boolean,
  } = {
      properties: [],
      failedCount: 0,
      failed: false,
    }

  propsBeingCheckedStr!: string;
  percentageFailed!: number;

  jsonProperties = [
    'Umbraco.Grid',
    'Umbraco.NestedContent',
  ];

  blockProperties = [
    'Umbraco.BlockList',
    'Umbraco.BlockGrid',
  ];

  noTests = false;
  notCreated = false;
  percentageDone = 20;
  progressStep = 0;
  activeVariant;

  constructor(
    private $scope,
    private $rootScope,
    private $element,
    private $timeout: ITimeoutService,
    private editorState,
    private preflightService: IPreflightService,
    private preflightHub: IPreflightHub)
  {
    this.validPropTypes = Umbraco.Sys.ServerVariables.Preflight.PropertyTypesToCheck;

    this.$scope.model.badge = {
      type: 'info'
    };    
  }


  /**
   * 
   * @param {any} arr
   */
  joinList = arr => {
    let outStr;
    if (arr.length === 1) {
      outStr = arr[0];
    } else if (arr.length === 2) {
      outStr = arr.join(' and ');
    } else if (arr.length > 2) {
      outStr = arr.slice(0, -1).join(', ') + ', and ' + arr.slice(-1);
    }

    return outStr;
  };


  /**
   * Convert a string to a hash for storage and comparison.
   * @param {string} s - the string to hashify
   * @returns {int} the generated hash
   */
  getHash = s => s ? s.split('').reduce((a, b) => {
    a = (a << 5) - a + b.charCodeAt(0);
    return a & a;
  }, 0) : 1;


  /**
   * Get property by alias from the current variant
   * @param {any} alias
   */
  getProperty = alias => {
    for (let tab of this.editorState.current.variants.find(x => x.active).tabs) {
      for (let prop of tab.properties) {
        if (prop.alias === alias) {
          return prop;
        }
      }
    }
  };


  /**
   * 
   * @param {any} editor
   */
  onComplete = () => {
    // it's possible no tests ran, in which case results won't exist
    this.noTests = this.results.properties.every(x => !x.plugins.length);

    if (this.noTests) {
      this.$scope.model.badge = undefined;
    }

    for (let p of this.results.properties) {
      p.disabled = p.failedCount === -1;
    }

    this.showSuccessMessage = !this.results.failed && !this.noTests && !this.notCreated;
    this.done = true;
  };


  /**
   * Updates the badge in the header with the number of failed tests
   */
  setBadgeCount = (pending?) => {
    if (pending) {
      this.$scope.model.badge = {
        type: 'warning'
      };
      return;
    }

    if (this.results && this.results.failedCount > 0) {
      this.$scope.model.badge = {
        count: this.results.failedCount,
        type: 'alert --error-badge pf-block'
      };
    } else {
      this.$scope.model.badge = {
        type: 'success icon-'
      };
    }
  };


  /**
   * if node is invariant, send no culture, otherwise get from language.name on the active variant
   * */
  getCurrentCulture = () => this.activeVariant.language ? this.activeVariant.language.culture : '';


  /**
   * Updates the property set with the new value, and removes any temporary property from that set
   * @param {object} data - a response model item returned via the signalr hub
   */
  rebindResult = (data: any) => {
    let newProp = true;
    let totalTestsRun = 0;
    let existingProp = this.results.properties.find(x => x.label === data.label);

    if (existingProp) {
      existingProp = Object.assign(existingProp, data);
      existingProp.loading = false;
      newProp = false;
    }

    // a new property will have a temporary placeholder - remove it
    // _temp ensures grid with multiple editors only removes the correct temp entry
    if (newProp && !data.remove && data.failedCount !== -1) {
      const tempIndex = this.results.properties.findIndex(p => p.name === `${data.name}_temp`);
      if (tempIndex !== -1) {
        this.results.properties.splice(tempIndex, 1);
      }
      this.results.properties.push(data);
    }

    this.results.properties = this.results.properties.filter(x => x.remove === false);
    this.results.properties = this.results.properties.filter(x => x.failedCount > -1);

    this.results.failedCount = this.results.properties.reduce((prev, cur) => {
      totalTestsRun += cur.totalTests;
      return prev + cur.failedCount;
    }, 0);

    this.results.failed = this.results.failedCount > 0;
    this.propsBeingCheckedStr = this.joinList(this.propsBeingChecked.splice(this.propsBeingChecked.indexOf(data.name), 1));
    this.percentageFailed = (totalTestsRun - this.results.failedCount) / totalTestsRun * 100;
  };


  /**
   * Finds dirty content properties, checks the type and builds a collection of simple models for posting to the preflight checkdirty endpoint
   * Also generates and stores a hash of the property value for comparison on subsequent calls, to prevent re-fetching unchanged data
   */
  checkDirty = () => {

    this.dirtyProps = [];
    this.hasDirty = false;

    for (let prop of this.propertiesToTrack) {
      const property = this.getProperty(prop.alias);

      if (!property) {
        return;
      }

      let currentValue = property.value;

      if (this.blockProperties.includes(prop.editor)) {
        currentValue = JSON.stringify(currentValue.contentData);
      } else if (this.jsonProperties.includes(prop.editor)) {
        currentValue = JSON.stringify(currentValue);
      }

      const hash = this.getHash(currentValue);

      if (this.dirtyHashes[prop.label] && this.dirtyHashes[prop.label] !== hash) {

        this.dirtyProps.push({
          name: prop.label,
          label: prop.label,
          value: currentValue,
          editor: prop.editor
        });

        this.dirtyHashes[prop.label] = hash;
        this.hasDirty = true;
      } else if (!this.dirtyHashes[prop.label]) {
        this.dirtyHashes[prop.label] = hash;
      }
    }

    // if dirty properties exist, create a simple model for each and send the lot off for checking
    // response comes via the signalr hub so is not handled here
    if (this.hasDirty) {
      this.$timeout(() => {

        this.dirtyProps.forEach(prop => {
          for (let existing of this.results.properties.filter(p => p.name === prop.name)) {
            if (existing) {
              existing.open = false;
              existing.failedCount = -1;
            } else {
              // generate new placeholder for pending results - this is removed when the response is returned
              this.results.properties.push({
                label: prop.name,
                open: false,
                failed: false,
                failedCount: -1,
                name: `${prop.name}_temp`
              });
            }
          }

          this.propsBeingChecked.push(prop.name);
        });

        this.propsBeingCheckedStr = this.joinList(this.propsBeingChecked);

        const payload = {
          properties: this.dirtyProps,
          culture: this.getCurrentCulture(),
          id: this.editorState.current.id
        };

        this.setBadgeCount(true);
        this.done = false;

        this.preflightService.checkDirty(payload);
      });
    }
  };

  /**
   * Initiates the signalr hub for returning test results
   */
  initSignalR = () => {

    this.preflightHub.initHub(hub => {
      const currentId = this.editorState.current.id;

      hub.on('preflightTest',
        e => {
          if (e.nodeId === currentId) {
            this.rebindResult(e);
            this.setBadgeCount();
          }
        });

      hub.on('preflightComplete',
        e => {
          if (e === currentId) {
            this.onComplete();
          }
        }
      );

      hub.start(() => {
        /**
         * Check all properties when the controller loads. Won't re-run when changing between apps
         * but needs to happen after the hub loads
         */
        this.$timeout(() => {
          this.setBadgeCount(true);
          this.checkDirty(); // builds initial hash array, but won't run anything                    
          this.preflightService.check(this.editorState.current.id, this.getCurrentCulture());
        });
      });
    });
  }

  $onInit = () => {
    this.$rootScope.$on('app.tabChange', (e, data) => {
      if (data.alias === 'preflight') {
        // collapse open nc controls, timeouts prevent $apply errors
        for (let openNc of Array.from(document.querySelectorAll('.umb-nested-content__item--active .umb-nested-content__header-bar'))) {
          this.$timeout(() => (openNc as any).click());
        }

        this.$timeout(() => {
          this.checkDirty();
          this.setBadgeCount();
        });
      }
    });

    this.$rootScope.$on('showPreflight', (event, data) => {
      if (data.nodeId === this.$scope.content.id) {
        // needs to find app closest to current scope
        const appLink = this.$element.closest('form').find('[data-element="sub-view-preflight"]');

        if (appLink) {
          appLink.click();
        }
      }
    });

    this.activeVariant = this.editorState.current.variants.find(x => x.active);
    this.propertiesToTrack = [];

    if (this.activeVariant) {
      this.notCreated = this.activeVariant.state === 'NotCreated';

      this.activeVariant.tabs.forEach(x => {
        this.propertiesToTrack = this.propertiesToTrack.concat(x.properties.map(x => {
          if (this.validPropTypes.includes(x.editor)) {
            return {
              editor: x.editor,
              alias: x.alias,
              label: x.label
            };
          }
        })).filter(x => x);
      });

      // array will have length, as app is only sent on types with testable properties
      if (this.propertiesToTrack.length) {
        this.initSignalR();
      }
    }
  }
}
