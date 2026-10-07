// Renders Markdown to HTML with the platform's bundled `marked` library.
// Use with ng-bind-html only: it passes the result through $sanitize, which strips scripts and event handlers.
angular.module('virtoCommerce.catalogQuality')
    .filter('catalogQualityMarkdown', function () {
        return function (text) {
            if (!text) {
                return '';
            }
            return window.marked.parse(text);
        };
    });
