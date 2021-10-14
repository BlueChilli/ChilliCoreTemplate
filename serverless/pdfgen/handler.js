'use strict';

process.env['PATH'] = process.env['PATH'] + ':' + '/tmp';

const fs = require('fs');
const wkhtmltopdf = require('wkhtmltopdf');
const MemoryStream = require('memorystream');

var initError = null;
function init(){
  try {
    if (!fs.existsSync('/tmp')){
      fs.mkdirSync('/tmp');
    }

    fs.copyFileSync('./wkhtmltopdf', '/tmp/wkhtmltopdf');
    fs.chmodSync('/tmp/wkhtmltopdf', 0o777); //chmod +x
  }
  catch(ierr){
    initError = ierr;
  }
}

function checkInitError(){
  if (initError){
    throw initError;
  }
}

function success(jsonBody){
  return {
    statusCode: 200,
    body: JSON.stringify(jsonBody)          
  };
}

function internalError(jsonBody){
  return {
    statusCode: 500,
    body: JSON.stringify(jsonBody)          
  };
}

init();
module.exports.generate = function(event, context, callback) {  
  try {  
    checkInitError();

    var inputBody = JSON.parse(event.body);
    var memStream = new MemoryStream();
    var htmlUtf8 = new Buffer(inputBody.htmlBase64, 'base64').toString('utf8');

    wkhtmltopdf(htmlUtf8, inputBody.options, function(code, signal) { 
      try {
        var pdfBase64 = memStream.read().toString('base64');

        callback(null, success({ pdfBase64: pdfBase64, code: code, signal: signal }));    
      }
      catch(ce){
        callback(null, internalError({ message: '' + ce.stack, code: code, signal: signal }));
      }
    }).pipe(memStream);
  }
  catch (e) {
    callback(null, internalError({ message: '' + e.stack }));    
  }
};