const colors = require('tailwindcss/colors')

module.exports = {
    purge: [],
    darkMode: false, // or 'media' or 'class'
    theme: {
        extend: {},
        colors: {
            grey: colors.warmGray,
            dark: {
                DEFAULT: "#181512",
                600: "#0e0d0b"
            },
            accent1: {
                100: "#f0ecdb",
                200: "#e1d9b7",
                300: "#d2c693",
                400: "#c3b36f",
                DEFAULT: "#b4a14b",
                600: "#90803c",
                700: "#6c602d",
                800: "#48401e",
                900: "#24200f"
            },
            accent2: {
                DEFAULT: "#630325",
                900: "#4a021b"
            },
            rose: colors.pink,
            "control": "#FEF3C7"
        },
        fontFamily: {
            "display": ['"Play"', '-apple-system', 'BlinkMacSystemFont', '"Segoe UI"', 'Roboto', '"Helvetica Neue"', 'Arial', '"Noto Sans"', 'sans-serif', '"Apple Color Emoji"', '"Segoe UI Emoji"', '"Segoe UI Symbol"', '"Noto Color Emoji"'],
            "body": ['"Roboto"', '-apple-system', 'BlinkMacSystemFont', '"Segoe UI"', 'Roboto', '"Helvetica Neue"', 'Arial', '"Noto Sans"', 'sans-serif', '"Apple Color Emoji"', '"Segoe UI Emoji"', '"Segoe UI Symbol"', '"Noto Color Emoji"']
        }
    },
    variants: {
        extend: {
            borderWidth: ["last"],
            margin: ["first"]
        },
    },
    plugins: [],
}
