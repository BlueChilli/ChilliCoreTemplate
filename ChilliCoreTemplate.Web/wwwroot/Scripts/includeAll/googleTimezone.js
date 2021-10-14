async function getTimezoneId(address, key) {
    var data = {
        location: address.geometry.latitude + ',' + address.geometry.longitude,
        timestamp: Math.floor(Date.now() / 1000),
        sensor: false,
        key: key
    };
    var timeZoneJson = await $.ajax({
        url: 'https://maps.googleapis.com/maps/api/timezone/json',
        dataType: 'json',
        data: data
    });
    return timeZoneJson.timeZoneId;
}
