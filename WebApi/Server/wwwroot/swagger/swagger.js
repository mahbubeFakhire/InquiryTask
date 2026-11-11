(function () {
    const overrider = () => {
        const swagger = window.ui;
        if (!swagger) {
            console.error('Swagger wasn\'t found');
            return;
        }

        clearInputPlaceHolder(swagger);
    }

    /**
  * Reset input element placeholder
  * @param {any} swagger
  */
    const clearInputPlaceHolder = (swagger) => {
        //https://github.com/api-platform/core/blob/main/src/Bridge/Symfony/Bundle/Resources/public/init-swagger-ui.js#L6-L41
        new MutationObserver(function (mutations, self) {
            var elements = document.querySelectorAll("input[type=text]");
            for (var i = 0; i < elements.length; i++)
                elements[i].placeholder = "";
        }).observe(document, { childList: true, subtree: true });
    }

    // append to event right after SwaggerUIBundle initialized
    window.addEventListener('load', () => setTimeout(overrider, 0), false);
}());