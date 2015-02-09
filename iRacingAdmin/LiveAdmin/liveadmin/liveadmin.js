$(document).ready(function() {

    var ssid = 0;
    var adminlist = $('#adminlist');
    var penaltytable = document.getElementById('penaltytable');

    var debug = true;

    function log(txt) {
        if (debug == true) {
            console.log('> LiveAdmin: ' + txt);
        }
    }
    
    // Penalty table header
    var header = '';
    for (var i = 0; i < columns.length; i++) {
        if (columns[i][2] > 0)
            header += '<th width="' + columns[i][2] + 'px">';
        else
            header += '<th width="100">';
        header += columns[i][1] + '</th>';
    }
    $('#penaltytable thead').append('<tr>' + header + '</tr>');
    

    // Load data 
    loadData = function() {

        log('Start loading');

        // Load current ssid
        loadCurrent();

        log('SSID: ' + ssid);

        if (ssid > 0) {
            // Get the data
            var url = sessionsDir + '/' + ssid + '.json';
            log(url);
            $.getJSON(url, function(data) {
                log('JSON: ' + JSON.stringify(data));
                updateData(data);
            });
        }
    };
    
    // Update data
    updateData = function(data) {
        var admins = data["Admins"];
        var penalties = data["Penalties"];

        // Set admin text
        var adminsText = '';
        for (var i = 0; i < admins.length; i++) {
            adminsText += admins[i]["Name"] + '(' + admins[i]["ShortName"] + ')';
            if (i < admins.length - 1) {
                adminsText += ', ';
            }
        }
        adminlist.text(adminsText);



        // Fill penalty table
        
        // Add rows / cells
        log('Adding rows / cells');
        if (penalties.length != penaltytable.tBodies[0].rows.length) {
            $('#penaltytable tbody').html('');
            for (var i = 0; i < penalties.length; i++) {
                var row = penaltytable.tBodies[0].insertRow(-1);
                for (var j = 0; j < columns.length; j++) {
                    var cell = row.insertCell(-1);
                }
            }
        }
        
        // Fill table
        log('Filling table');
        for (var i = 0; i < penalties.length; i++) {
            var row = penaltytable.tBodies[0].rows[i];
            var penalty = penalties[i];
            
            for (var j = 0; j < columns.length; j++) {
                var col = columns[j];
                var property = col[0];

                var value = penalty[property];
                if (col.length == 4) {
                    var func = col[3];
                    value = func(value);
                }

                row.cells[j].innerHTML = value;
            }
        }
        log('Finished');
    };
    
    // Load current ssid
    loadCurrent = function() {
        var url = sessionsDir + '/current.txt';
        $.get(url, function(data) {
            ssid = data;
        });
    };

    setInterval(loadData, updateFrequency * 1000);
    loadData();
});

