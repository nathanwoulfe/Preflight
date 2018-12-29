(() => {

    function preflightService($http, umbRequestHelper) {

        const urlBase = Umbraco.Sys.ServerVariables.preflight.apiPath;

        const helpText = `
            <p>If your content is too difficult for your visitors to read, you're all going to have a bad time.</p>
            <p>The Preflight readability test runs your content through the Flesch reading ease algorithm to determine text complexity.</p>
            <h5>The algorithm</h5>
            <p><code>RE = 206.835 - (1.015 x ASL) - (84.6 x ASW)</code></p>
            <p>Where RE is Readability Ease, ASL is Average Sentence Length, and ASW is Average Syllables per Word</p>
            <p>The result is a number between 0 and 100, where a higher score means better readability, with a score between 60 and 69 largely considered acceptable.</p>
            <h5>Preflight test results</h5>
            <p>As well as the Flesch score, Preflight returns sentence length; average syllables per word; long or complex words; and broken links (an external link returning a non-200 code response, or a relative internal link).</p>
            <p>Links are also submitted to Google's SafeBrowsing API, if an API key has been provided.</p>
            <p>The package currently supports RTE editors nested in Grid or Archetype editors, or added as standalone properties.</p>`;

        const request = (method, url, data) =>
            umbRequestHelper.resourcePromise(
                method === 'GET' ? $http.get(url) : $http.post(url, data),
                'Something broke'
            );

        const service = {
            check: id => request('GET', `${urlBase}check/${id}`),

            getSettings: () => request('GET', `${urlBase}getSettings`),

            saveSettings: settings => request('POST', `${urlBase}saveSettings`, settings),

            getHelpText: () => helpText
        };

        return service;
    }

    angular.module('umbraco').service('preflightService', ['$http', 'umbRequestHelper', preflightService]);

})();