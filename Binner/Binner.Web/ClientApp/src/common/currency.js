import { getCurrencySymbol } from "./Utils";

export const Currencies = [
  { key: 0, text: getCurrencySymbol("USD"), value: "USD", description: "US Dollar" },
  { key: 1, text: getCurrencySymbol("EUR"), value: "EUR", description: "Euro" },
  { key: 2, text: getCurrencySymbol("CAD"), value: "CAD", description: "Canadian dollar" },
  { key: 3, text: getCurrencySymbol("JPY"), value: "JPY", description: "Japanese yen" },
  { key: 4, text: getCurrencySymbol("GBP"), value: "GBP", description: "Pound sterling" },
  { key: 5, text: getCurrencySymbol("AUD"), value: "AUD", description: "Australian dollar" },
  { key: 6, text: getCurrencySymbol("CAD"), value: "CNY", description: "Renminbi" },
  { key: 7, text: getCurrencySymbol("KRW"), value: "KRW", description: "South Korean won" },
  { key: 8, text: getCurrencySymbol("MXN"), value: "MXN", description: "Mexican peso" }
];
