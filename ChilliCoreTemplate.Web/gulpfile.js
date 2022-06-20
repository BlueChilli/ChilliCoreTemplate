/// <binding Clean='clean, min' ProjectOpened='clean, min, watch' />
"use strict";

var fs = require("fs"),
    gulp = require("gulp"),
    concat = require("gulp-concat"),
    cssmin = require("gulp-cssmin"),
    htmlmin = require("gulp-htmlmin"),
    terser = require("gulp-terser"),
    sass = require('gulp-sass')(require('node-sass')),
    merge = require("merge-stream"),
    del = require("del"),
    stripJsonComments = require("strip-json-comments"),
    path = require("path");


var bundleData = stripJsonComments(
    fs.readFileSync("./bundleconfig.json", "utf8")
);
var bundleconfig = JSON.parse(bundleData);

var regex = {
    css: /\.css$/,
    html: /\.(html|htm)$/,
    js: /\.js$/,
    fonts: /\.assets$/
};

gulp.task("min:js", function () {
    var tasks = getBundles(regex.js).map(function (bundle) {
        return gulp
            .src(bundle.inputFiles, { base: "." })
            .pipe(concat(bundle.outputFileName))
            .pipe(
                terser({
                    mangle: false,
                    ecma: 6
                })
            )
            .on("error", function (e) {
                console.log(e);
            })
            .pipe(gulp.dest("."));
    });
    return merge(tasks);
});

gulp.task("min:css", function () {
    var tasks = getBundles(regex.css).map(function (bundle) {
        return gulp
            .src(bundle.inputFiles, { base: "." })
            .pipe(sass().on("error", sass.logError))
            .pipe(concat(bundle.outputFileName))
            .pipe(cssmin())
            .pipe(gulp.dest("."));
    });
    return merge(tasks);
});

gulp.task("min:html", function () {
    var tasks = getBundles(regex.html).map(function (bundle) {
        return gulp
            .src(bundle.inputFiles, { base: "." })
            .pipe(concat(bundle.outputFileName))
            .pipe(
                htmlmin({ collapseWhitespace: true, minifyCSS: true, minifyJS: true })
            )
            .pipe(gulp.dest("."));
    });
    return merge(tasks);
});

gulp.task("clean", function () {
    var files = bundleconfig.map(function (bundle) {
        return bundle.outputFileName;
    });

    return del(files);
});

gulp.task("assets", function () {
    var tasks = getBundles(regex.fonts).map(function (bundle) {
        return gulp.src(bundle.inputFiles).pipe(gulp.dest(bundle.outputFolder));
    });
    return merge(tasks);
});

gulp.task("min", gulp.series("assets", "min:js", "min:css")); //, "min:html" fails due to no files matching this pattern
gulp.task("default", gulp.series("clean", "min"));

gulp.task("watch", function () {
    getBundles(regex.js).forEach(function (bundle) {
        gulp.watch(bundle.inputFiles, ["min:js"]);
    });

    getBundles(regex.css).forEach(function (bundle) {
        gulp.watch(bundle.inputFiles, ["min:css"]);
    });

    getBundles(regex.html).forEach(function (bundle) {
        gulp.watch(bundle.inputFiles, ["min:html"]);
    });
});

function getBundles(regexPattern) {
    return bundleconfig.filter(function (bundle) {
        return regexPattern.test(bundle.outputFileName);
    });
}
