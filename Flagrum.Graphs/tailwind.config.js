const colors = require('tailwindcss/colors')

module.exports = {
  purge: [],
  darkMode: false, // or 'media' or 'class'
  theme: {
      extend: {},
      colors: {
          grey: colors.coolGray,
          dark: {
              DEFAULT: "rgb(0, 10, 25)",
              700: "rgb(0, 5, 10)"
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
