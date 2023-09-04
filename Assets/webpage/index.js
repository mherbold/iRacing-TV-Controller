const maxPositionsPerLeaderboard = 64;

var app =
{
    nextJsonFileIndex: 0,

    liveDataA: null,
    liveDataB: null,

    delta: 0.0,

    dom:
    {
        liveDataContainer: null,

        leaderboard:
        {
            template: null,
            positionTemplate: null,
        },

        leaderboards:
        {
            container: null
        },

        trackMap:
        {
            canvas: null,
        },

        eventLog:
        {
            messageTemplate: null,
            container: null
        },

        raceStatus:
        {
            blackLight: null,
            greenLight: null,
            yellowLight: null,
            whiteLight: null,
            greenFlag: null,
            yellowFlag: null,
            checkeredFlag: null,
            sessionName: null,
            lapsRemaining: null,
            units: null,
            currentLap: null
        }
    },

    setting:
    {
        updateInterval: 0.5,
        numJsonFiles: 3,

        trackMap:
        {
            lineWidth: 5,
            strokeStyle: "#000",
            startFinishSize: 3,
            startFinishFillStyle: "#E00",
            carSize: 10,
            carFillStyle: "#fff",
            carStrokeStyle: "#000",
            carTextStyle: "#000"
        }
    }
};

function updateLeaderboards( liveDataLeaderboards )
{
    // find all current dom leaderboards

    var domLeaderboards = app.dom.leaderboards.container.querySelectorAll( ".leaderboard" );
    var domLeaderboardIndex = 0;

    // remove obsolete dom leaderboards

    domLeaderboards.forEach( ( domLeaderboard ) =>
    {
        domLeaderboardIndex++;

        if ( domLeaderboardIndex > liveDataLeaderboards.length )
        {
            domLeaderboard.remove();
        }
    } );

    // add new dom leaderboards

    while ( domLeaderboardIndex < liveDataLeaderboards.length )
    {
        var domLeaderboard = app.dom.leaderboard.template.cloneNode( true );

        domLeaderboard.classList.remove( "d-none" );

        app.dom.leaderboards.container.append( domLeaderboard );

        domLeaderboardIndex++;

        var domLeaderboardPositionsContainer = domLeaderboard.querySelector( ".leaderboard-positions-container" );

        for ( var i = 0; i < maxPositionsPerLeaderboard; i++ )
        {
            var domLeaderboardPosition = app.dom.leaderboard.positionTemplate.cloneNode( true );

            domLeaderboardPosition.classList.remove( "d-none" );

            domLeaderboardPositionsContainer.append( domLeaderboardPosition );
        }
    }

    // re-find all current dom leaderboards

    domLeaderboards = app.dom.leaderboards.container.querySelectorAll( ".leaderboard" );

    // update each dom leaderboard

    var liveDataLeaderboardIndex = 0;

    domLeaderboards.forEach( ( domLeaderboard ) =>
    {
        var liveDataLeaderboard = liveDataLeaderboards[ liveDataLeaderboardIndex++ ];

        var domLeaderboardClassName = domLeaderboard.querySelector( ".leaderboard-class-name" );

        domLeaderboardClassName.textContent = liveDataLeaderboard.className;

        var domLeaderboardPositions = domLeaderboard.querySelectorAll( ".leaderboard-position" );

        var domLeaderboardPositionIndex = 0;

        domLeaderboardPositions.forEach( ( domLeaderboardPosition ) =>
        {
            var liveDataLeaderboardSlots = liveDataLeaderboard.liveDataLeaderboardSlots;

            var liveDataLeaderboardSlotFound = false;

            for ( var liveDataLeaderboardSlotIndex = 0; liveDataLeaderboardSlotIndex < liveDataLeaderboardSlots.length; liveDataLeaderboardSlotIndex++ )
            {
                var liveDataLeaderboardSlot = liveDataLeaderboardSlots[ liveDataLeaderboardSlotIndex ];

                if ( liveDataLeaderboardSlot.show )
                {
                    var targetDomLeaderboardPositionIndex = parseInt( liveDataLeaderboardSlot.positionText ) - 1;

                    if ( targetDomLeaderboardPositionIndex == domLeaderboardPositionIndex )
                    {
                        domLeaderboardPosition.classList.remove( "d-none" );

                        var domLeaderboardPositionPosition = domLeaderboardPosition.querySelector( ".leaderboard-position-position" );
                        domLeaderboardPositionPosition.textContent = liveDataLeaderboardSlot.positionText;

                        var domLeaderboardCarNumber = domLeaderboardPosition.querySelector( ".leaderboard-position-car-number" );
                        domLeaderboardCarNumber.textContent = liveDataLeaderboardSlot.carNumberText;

                        var domLeaderboardDriverName = domLeaderboardPosition.querySelector( ".leaderboard-position-driver-name" );
                        domLeaderboardDriverName.textContent = liveDataLeaderboardSlot.driverNameText;

                        var domLeaderboardTelemetry = domLeaderboardPosition.querySelector( ".leaderboard-position-telemetry" );
                        domLeaderboardTelemetry.textContent = liveDataLeaderboardSlot.telemetryText;

                        var domLeaderboardSpeed = domLeaderboardPosition.querySelector( ".leaderboard-position-speed" );
                        domLeaderboardSpeed.textContent = liveDataLeaderboardSlot.speedText;

                        liveDataLeaderboardSlotFound = true;

                        break;
                    }
                }
            }

            if ( !liveDataLeaderboardSlotFound )
            {
                domLeaderboardPosition.classList.add( "d-none" );
            }

            domLeaderboardPositionIndex++;
        } );
    } );
}

function toggleVisibility( element, visible )
{
    if ( visible )
    {
        element.classList.remove( "d-none" );
    }
    else
    {
        element.classList.add( "d-none" );
    }
}

function updateTrackMap( liveDataTrackMap )
{
    // update canvas size

    app.dom.trackMap.canvas.width = app.dom.trackMap.canvas.offsetWidth;
    app.dom.trackMap.canvas.height = app.dom.trackMap.canvas.offsetHeight;

    // calculate scale and offset of track map

    var padding = Math.max( app.setting.trackMap.lineWidth / 2, app.setting.trackMap.carSize + 1 ) * 2 + 2;

    var liveDataTrackMapWidthHeightRatio = liveDataTrackMap.height / liveDataTrackMap.width;

    if ( ( app.dom.trackMap.canvas.width - padding ) * liveDataTrackMapWidthHeightRatio <= ( app.dom.trackMap.canvas.height - padding ) )
    {
        scale = app.dom.trackMap.canvas.width - padding;
    }
    else
    {
        scale = ( app.dom.trackMap.canvas.height - padding ) / liveDataTrackMapWidthHeightRatio;
    }

    var width = scale;
    var height = scale * liveDataTrackMapWidthHeightRatio;

    var offsetX = ( app.dom.trackMap.canvas.width - width ) / 2;
    var offsetY = ( app.dom.trackMap.canvas.height - height ) / 2;

    // draw track map

    var ctx = app.dom.trackMap.canvas.getContext( "2d" );

    ctx.font = "10px sans-serif";
    ctx.textAlign = "center";
    ctx.textBaseline = "middle";

    ctx.clearRect( 0, 0, app.dom.trackMap.canvas.width, app.dom.trackMap.canvas.height );

    ctx.beginPath();

    var drawVectorList = liveDataTrackMap.drawVectorList;

    var lastDrawVector = drawVectorList[ drawVectorList.length - 1 ];

    ctx.moveTo( lastDrawVector.x * scale + offsetX, lastDrawVector.y * -scale + offsetY );

    for ( var i = 0; i < drawVectorList.length; i++ )
    {
        var drawVector = drawVectorList[ i ];

        ctx.lineTo( drawVector.x * scale + offsetX, drawVector.y * -scale + offsetY );
    }

    ctx.closePath();

    // draw start/finish line

    ctx.lineWidth = app.setting.trackMap.lineWidth;
    ctx.strokeStyle = app.setting.trackMap.strokeStyle;

    ctx.stroke();

    var startFinishLine = liveDataTrackMap.startFinishLine;

    ctx.beginPath();

    ctx.arc( startFinishLine.x * scale + offsetX, startFinishLine.y * -scale + offsetY, app.setting.trackMap.startFinishSize, 0, 2 * Math.PI );

    ctx.fillStyle = app.setting.trackMap.startFinishFillStyle;

    ctx.fill();

    // draw cars

    var liveDataTrackMapCars = liveDataTrackMap.liveDataTrackMapCars;

    for ( var i = 0; i < liveDataTrackMapCars.length; i++ )
    {
        var liveDataTrackMapCar = liveDataTrackMapCars[ i ];

        if ( liveDataTrackMapCar.show )
        {
            ctx.beginPath();

            var x = liveDataTrackMapCar.offset.x * scale + offsetX;
            var y = liveDataTrackMapCar.offset.y * -scale + offsetY;

            ctx.arc( x, y, app.setting.trackMap.carSize, 0, 2 * Math.PI );

            ctx.fillStyle = app.setting.trackMap.carFillStyle;

            ctx.fill();

            ctx.lineWidth = 1;
            ctx.strokeStyle = app.setting.trackMap.carStrokeStyle;

            ctx.stroke();

            ctx.fillStyle = app.setting.trackMap.carTextStyle;

            ctx.fillText( liveDataTrackMapCar.carNumber, x, y );
        }
    }
}

function updateEventLog( liveDataEventLog )
{
    while ( app.dom.eventLog.container.firstChild )
    {
        app.dom.eventLog.container.removeChild( app.dom.eventLog.container.firstChild );
    }

    var domEventLogMessage = null;

    for ( var i = 0; i < liveDataEventLog.messages.length; i++ )
    {
        domEventLogMessage = app.dom.eventLog.messageTemplate.cloneNode( true );

        var typeAsClass = "type-" + liveDataEventLog.messages[ i ].type.toLowerCase().replace( " ", "-" );

        domEventLogMessage.classList.remove( "d-none" );
        domEventLogMessage.classList.add( typeAsClass );

        var domEventLogMessageSessionTime = domEventLogMessage.querySelector( ".event-log-message-session-time" );
        domEventLogMessageSessionTime.textContent = liveDataEventLog.messages[ i ].sessionTime;

        var domEventLogMessageCarNumber = domEventLogMessage.querySelector( ".event-log-message-car-number" );
        domEventLogMessageCarNumber.textContent = liveDataEventLog.messages[ i ].carNumber;

        var domEventLogMessageDriverName = domEventLogMessage.querySelector( ".event-log-message-driver-name" );
        domEventLogMessageDriverName.textContent = liveDataEventLog.messages[ i ].driverName;

        var domEventLogMessagePosition = domEventLogMessage.querySelector( ".event-log-message-position" );
        domEventLogMessagePosition.textContent = liveDataEventLog.messages[ i ].position;

        var domEventLogMessageType = domEventLogMessage.querySelector( ".event-log-message-type" );
        domEventLogMessageType.textContent = liveDataEventLog.messages[ i ].type;

        var domEventLogMessageText = domEventLogMessage.querySelector( ".event-log-message-text" );
        domEventLogMessageText.textContent = liveDataEventLog.messages[ i ].text;

        app.dom.eventLog.container.prepend( domEventLogMessage );
    }
}

function updateRaceStatus( liveDataRaceStatus )
{
    toggleVisibility( app.dom.raceStatus.greenFlag, liveDataRaceStatus.showGreenFlag );
    toggleVisibility( app.dom.raceStatus.yellowFlag, liveDataRaceStatus.showYellowFlag );
    toggleVisibility( app.dom.raceStatus.checkeredFlag, liveDataRaceStatus.showCheckeredFlag );

    toggleVisibility( app.dom.raceStatus.blackLight, liveDataRaceStatus.showBlackLight );
    toggleVisibility( app.dom.raceStatus.greenLight, liveDataRaceStatus.showGreenLight );
    toggleVisibility( app.dom.raceStatus.yellowLight, liveDataRaceStatus.showYellowLight );
    toggleVisibility( app.dom.raceStatus.whiteLight, liveDataRaceStatus.showWhiteLight );

    app.dom.raceStatus.sessionName.textContent = liveDataRaceStatus.sessionNameText;
    app.dom.raceStatus.lapsRemaining.textContent = liveDataRaceStatus.lapsRemainingText;
    app.dom.raceStatus.units.textContent = liveDataRaceStatus.unitsText;
    app.dom.raceStatus.currentLap.textContent = liveDataRaceStatus.currentLapText;
}

function update( liveData )
{
    if ( liveData.isConnected )
    {
        updateLeaderboards( liveData.liveDataLeaderboardsWebPage );
        updateTrackMap( liveData.liveDataTrackMap );
        updateEventLog( liveData.liveDataEventLog );
        updateRaceStatus( liveData.liveDataRaceStatus );
    }
}

function render()
{
    if ( ( app.liveDataA !== null ) && ( app.liveDataB !== null ) )
    {
        if ( app.liveDataA.isConnected && app.liveDataB.isConnected )
        {
            var oneMinusDelta = 1.0 - app.delta;

            var liveData = JSON.parse( JSON.stringify( app.liveDataA ) );

            var carsA = liveData.liveDataTrackMap.liveDataTrackMapCars;
            var carsB = app.liveDataB.liveDataTrackMap.liveDataTrackMapCars;

            for ( var i = 0; i < carsA.length; i++ )
            {
                var carA = carsA[ i ];
                var carB = carsB[ i ];

                carA.offset.x = carA.offset.x * oneMinusDelta + carB.offset.x * app.delta;
                carA.offset.y = carA.offset.y * oneMinusDelta + carB.offset.y * app.delta;
            }

            update( liveData );

            app.delta += 0.1;
        }
    }
}

async function tick()
{
    var response = await fetch( `livedata${app.nextJsonFileIndex}.json` );

    if ( response.ok )
    {
        app.nextJsonFileIndex = ( app.nextJsonFileIndex + 1 ) % app.setting.numJsonFiles;

        app.liveDataA = app.liveDataB;
        app.liveDataB = await response.json();

        app.delta = 0.0;
    }
}

function init()
{
    app.dom.liveDataContainer = document.querySelector( ".live-data-container" );

    app.dom.leaderboard.template = document.querySelector( ".leaderboard-template" );
    app.dom.leaderboard.positionTemplate = document.querySelector( ".leaderboard-position-template" );

    app.dom.leaderboards.container = document.querySelector( ".leaderboards-container" );

    app.dom.trackMap.canvas = document.querySelector( ".track-map-canvas" );

    app.dom.eventLog.messageTemplate = document.querySelector( ".event-log-message-template" );
    app.dom.eventLog.container = document.querySelector( ".event-log-container" );

    app.dom.raceStatus.blackLight = document.querySelector( ".race-status-black-light" );
    app.dom.raceStatus.greenLight = document.querySelector( ".race-status-green-light" );
    app.dom.raceStatus.yellowLight = document.querySelector( ".race-status-yellow-light" );
    app.dom.raceStatus.whiteLight = document.querySelector( ".race-status-white-light" );
    app.dom.raceStatus.greenFlag = document.querySelector( ".race-status-green-flag" );
    app.dom.raceStatus.yellowFlag = document.querySelector( ".race-status-yellow-flag" );
    app.dom.raceStatus.checkeredFlag = document.querySelector( ".race-status-checkered-flag" );
    app.dom.raceStatus.sessionName = document.querySelector( ".race-status-session-name" );
    app.dom.raceStatus.lapsRemaining = document.querySelector( ".race-status-laps-remaining" );
    app.dom.raceStatus.units = document.querySelector( ".race-status-units" )
    app.dom.raceStatus.currentLap = document.querySelector( ".race-status-current-lap" );

    app.setting.trackMap.lineWidth = parseFloat( app.dom.trackMap.canvas.getAttribute( "data-line-width" ) );
    app.setting.trackMap.strokeStyle = app.dom.trackMap.canvas.getAttribute( "data-stroke-style" );
    app.setting.trackMap.startFinishSize = parseFloat( app.dom.trackMap.canvas.getAttribute( "data-start-finish-size" ) );
    app.setting.trackMap.startFinishFillStyle = app.dom.trackMap.canvas.getAttribute( "data-start-finish-fill-style" );
    app.setting.trackMap.carSize = parseFloat( app.dom.trackMap.canvas.getAttribute( "data-car-size" ) );
    app.setting.trackMap.carFillStyle = app.dom.trackMap.canvas.getAttribute( "data-car-fill-style" );
    app.setting.trackMap.carStrokeStyle = app.dom.trackMap.canvas.getAttribute( "data-car-stroke-style" );
    app.setting.trackMap.carTextStyle = app.dom.trackMap.canvas.getAttribute( "data-car-text-style" );

    var timeout = Math.round( app.setting.updateInterval * 1000 );

    setInterval( tick, timeout );
    setInterval( render, 50 );
}

function ready( fn )
{
    if ( document.readyState !== 'loading' )
    {
        fn();
    }
    else
    {
        document.addEventListener( 'DOMContentLoaded', fn );
    }
}

ready( init );
