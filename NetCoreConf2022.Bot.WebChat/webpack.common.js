/* eslint-disable */

const path = require("path")
const webpackUtf8Bom = require('webpack-utf8-bom');

/* eslint-enable */

module.exports = {
    entry: path.resolve(__dirname, './src/bot-webchat.js'),
    output: {
        clean: true,
        filename: 'bot-webchat.js',
        path: path.resolve(__dirname, './dist'),        
    },
    plugins: [
        new webpackUtf8Bom(true),       
    ]
};
