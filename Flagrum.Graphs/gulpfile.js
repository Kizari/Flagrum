/// <binding BeforeBuild='SCSSModules, AppStyles' ProjectOpened='Watch' />
"use strict";

const gulp = require("gulp");
const postcss = require("gulp-postcss");

const appStyles = () => {
    return gulp.src("./Styles/app.css")
        .pipe(postcss([
            require("precss"),
            require("tailwindcss"),
            require("autoprefixer")
        ]))
        .pipe(gulp.dest("./wwwroot"));
}

const watch = () => {
    gulp.watch(["./Styles/*.css", "./tailwind.config.js"], gulp.series([appStyles]));
};

gulp.task("Watch", watch);
gulp.task("AppStyles", appStyles);