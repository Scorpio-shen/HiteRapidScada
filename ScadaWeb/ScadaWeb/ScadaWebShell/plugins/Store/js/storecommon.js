// Path to the Store service
var API_PATH1 = "http://rapidscada.net/minicrm/api/v1/";
var API_PATH2 = "http://localhost:64556/api/v1/";
var API_PATH = API_PATH1;

// The variables below must be defined in *.aspx
// User locale or culture
var locale = locale || "";
// Localized phrases
var phrases = phrases || {};

// Get formatted string representation of the module price
function formatPrice(module) {
    if (module.PersonalPrice || module.BusinessPrice) {
        if (module.PersonalPriceStr === module.BusinessPriceStr) {
            return module.PersonalPriceStr;
        } else {
            var personalText = phrases.PersonalPrice + module.PersonalPriceStr;
            var businessText = phrases.BusinessPrice + module.BusinessPriceStr;

            if (module.PersonalPrice <= 0) {
                return businessText;
            } else if (module.BusinessPrice <= 0) {
                return personalText;
            } else {
                return personalText + "\n" + businessText;
            }
        }
    } else {
        return phrases.FreePrice;
    }
}