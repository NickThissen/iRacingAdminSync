function convertTime(milliseconds) {
    var date = new Date(milliseconds);
    return date.getHours() + ":" + date.getMinutes();
}