# Preflight - Pre-publishing checks for Umbraco
Preflight subjects your content to series of QA test prior to publishing:

- Reading ease
- Link check
- Text blacklisting

The generated report includes average sentence length, average syllables per word, long or complex words, link status and blacklisted words.

Links are also sent to Google's SafeBrowsing API to check for malware and similar.

Readability levels are configurable, as are the syllable count for defining a long word, whitelisted terms which are ignored in all checks, and blacklisted terms which are never permitted.

Preflight can be run on demand from the context menu, or set to run as a before-save event (the save event is cancelled if the page fails any checks).

The package currently supports content in RTE editors, including those nested in Grid or Archetype editors. 