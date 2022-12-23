interface IProperty {
  name: string;
  editor: string;
  alias?: string;
  label: string;
  value: any;
}

interface ISetting {
  alias: string;
  view: string;
  value: {
    key?: string;
    value?: any;
  } | [] | null,
  prevalues: Array<ISettingConfigPrevalue>;
  validation?: any;
  config: {
    handle?: string;
    initVal1?: number;
    maxVal?: number;
    minVal?: number;
    orientation?: string;
    step?: number;
    tooltip?: string;
    tooltipPosition?: string;
    min?: number;
    max?: number;
    items?: Array<ISettingConfigPrevalue>;
  }
}

interface ISettingConfigPrevalue {

}

interface IPreflightSettingsResponse {

}

interface IPreflightService {
  getSettings: () => Promise<IPreflightSettingsResponse>;
  saveSettings: (data: Array<ISetting>, tabs) => Promise<void>;
  check: (id: number, culture: string) => Promise<any>;
  checkDirty: (data: any) => Promise<any>;
}

interface IPreflightHub {
  initHub: (any) => void;
}
