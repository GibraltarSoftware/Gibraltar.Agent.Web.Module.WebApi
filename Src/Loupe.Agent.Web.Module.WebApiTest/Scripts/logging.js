function Logging() {

    var token = null;
    var requestHeader = loupe.clientSessionHeader();
    var agentSessionId = requestHeader.headerValue;

    $('#AgentSessionId').text(agentSessionId);
    updateSequeceNumber();

    return {
        verbose: logVerbose,
        information: logInformation,
        warning: logWarning,
        error: logError,
        critical: logCritical,
        write: logWrite,
        unhandledException: throwUnhandledException,
        ajaxCall: makeAjaxCall,
        login: login
    }

    function logVerbose() {
        var data = getData();
        loupe.verbose(data.category, data.caption, data.description, data.parameters, data.exception, data.details, data.methodSourceInfo);
        updateSequeceNumber();
    }

    function logInformation() {
        var data = getData();
        loupe.information(data.category, data.caption, data.description, data.parameters, data.exception, data.details, data.methodSourceInfo);
        updateSequeceNumber();
    }

    function logWarning() {
        var data = getData();
        loupe.warning(data.category, data.caption, data.description, data.parameters, data.exception, data.details, data.methodSourceInfo);
        updateSequeceNumber();
    }

    function logError() {
        var data = getData();
        loupe.error(data.category, data.caption, data.description, data.parameters, data.exception, data.details, data.methodSourceInfo);
        updateSequeceNumber();
    }

    function logCritical() {
        var data = getData();
        loupe.critical(data.category, data.caption, data.description, data.parameters, data.exception, data.details, data.methodSourceInfo);
        updateSequeceNumber();
    }

    function logWrite() {
        var data = getData();
        var severity = $('#severity').val();
        loupe.write(severity, data.category, data.caption, data.description, data.parameters, data.exception, data.details, data.methodSourceInfo);
        updateSequeceNumber();
    }

    function throwUnhandledException() {
        var foo = {};
        setTimeout(updateSequeceNumber, 15);
        foo.bar();
    }

    function makeAjaxCall() {
        $('#ajaxCallResult').text("");

        var loupeHeader = {};
        loupeHeader[requestHeader.headerName] = requestHeader.headerValue;

        if (token) {
            loupeHeader["Authorization"] = "Basic " + token;
        }

        $.ajax({
            type: "GET",
            url: '/api/my/data',
            headers: loupeHeader
        }).done(function (data) {
            $('#ajaxCallResult').text("succeeded");
        }).error(function(jqXHR, textStatus) {
            $('#ajaxCallResult').text("failed:" + jqXHR.status + " " + jqXHR.statusText);
        });
    }

    function login() {
        $('#loginStatus').text('Attempting login');

        token = createBasicAuthToken();

        makeRequest('/api/account/token').done(function (result) {
            $('#loginStatus').text('Logged in');
            loupe.setAuthorisationHeader({ name: "Authorization", value: "Basic " + token });
        }).fail(function (result) {
            $('#loginStatus').text('Error logging in');
        });
    }

    function makeRequest(url) {
        var ajaxSettings = {
            type: 'GET',
            url: url,
            beforeSend: function (xhr) {
                xhr.setRequestHeader("Authorization", "Basic " + token);
            }
        };

        return $.ajax(ajaxSettings);
    }

    function createBasicAuthToken() {
        var uid = "jane_smith";
        var pwd = "jane_smith";

        // the btoa call isn't ideal due to browser differeneces
        // but is sufficent for this naive example
        return btoa(uid + ':' + pwd);
    }

    function getInputVal(inputName) {
        return $(inputName).val();
    }

    function createMethodSourceInfo() {
        var file = getInputVal("#fileInput");
        var method = getInputVal('#methodInput');
        var line = getInputVal("#lineInput");
        var column = getInputVal("#columnInput");

        if (file || method || line || column) {
            return new loupe.MethodSourceInfo(file, method, line, column);
        }

        return null;
    }

    function getData() {

        var parameters = null;
        var parametersValue = getInputVal('#parametersInput');
        if (parametersValue) {
            parameters = parametersValue.split(',');
        }

        var exception = null;
        var exceptionMessage = getInputVal('#exceptionMessageInput');
        if (exceptionMessage) {
            exception = new Error(exceptionMessage);
        }

        var methodSourceInfo = createMethodSourceInfo();
        

        return {
            category: getInputVal("#categoryInput"),
            caption: getInputVal("#captionInput"),
            description: getInputVal("#descriptionInput"),
            parameters: parameters,
            details: getInputVal("#detailsInput"),
            exception: exception,
            methodSourceInfo: methodSourceInfo
        };
    }

    function updateSequeceNumber() {
        var sequenceNumber = "";
        try {
            sequenceNumber = sessionStorage.getItem("LoupeSequenceNumber") || "0";
            
        } catch (e) {
            // seems we can't get it from sessionStorage
        }

        $('#sequenceNumber').text(sequenceNumber);
    }

}
