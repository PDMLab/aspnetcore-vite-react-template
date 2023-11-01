module.exports = {
  content: [
    './src/**/*.{js,jsx,ts,tsx}',
    '../Features/**/*.cshtml',
    "../Areas/**/*.{cshtml,js}",
    './*.html'
  ],
  safelist: [
    'field-validation-error',
    'input-validation-error',
    'validation-summary-errors'
  ],
  plugins: [
    require("@tailwindcss/forms"),
    require('tailwindcss-debug-screens'),
  ],
  theme: {
    extend: {
      colors: {
        blue: {
          50: "#EBF8FF",
          100: "#EBF8FF",
          200: "#D0EFFF",
          300: "#B5E6FE",
          400: "#7FD4FD",
          500: "#12B0FB",
          600: "#095F88",
          700: "#05374E",
          800: "#032331",
          900: "#000E14",
        },
      },
    },
    debugScreens: {
      position: ['bottom', 'left'],
    },
  },
  variants: {
    extend: {
      opacity: ["disabled"],
    },
  },
};
