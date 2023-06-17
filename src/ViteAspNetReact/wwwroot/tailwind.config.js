module.exports = {
  content: [
    './src/**/*.{js,jsx,ts,tsx}',
    './*.html'
  ],
  safelist: [
    'field-validation-error',
    'input-validation-error',
    'validation-summary-errors'
  ],
  plugins: [
    // eslint-disable-next-line @typescript-eslint/no-var-requires
    require("@tailwindcss/forms")({
      strategy: "class",
    }),
    require("tailwindcss-debug-screens"),
  ],
  theme: {
    extend: {
      colors: {
        turquoise: {
          100: "#d8f3fd",
          200: "#77d6f8",
          300: "#51cbf7",
          400: "#51cbf7",
          500: "#51cbf7",
          600: "#08749B",
          700: "#065775",
          800: "#51cbf7",
          900: "#021d27",
        },
      },
    },
  },
  variants: {
    extend: {
      opacity: ["disabled"],
    },
  },
};
