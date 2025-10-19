import { useTranslation } from "react-i18next";

export default function LanguageSwitcher() {
  const { i18n, t } = useTranslation();
  const current = i18n.language.startsWith("en") ? "en" : "cn";

  const setLang = (lng: "en" | "cn") => {
    i18n.changeLanguage(lng);
    localStorage.setItem("i18nextLng", lng);
    document.documentElement.lang = lng === "cn" ? "zh" : "en";
  };

  return (
    <div className="btn-group">
      <button
        className={`btn btn-sm ${current === "cn" ? "btn-danger" : "btn-outline-danger"}`}
        onClick={() => setLang("cn")}
      >
        {t("lang.cn")}
      </button>
      <button
        className={`btn btn-sm ${current === "en" ? "btn-danger" : "btn-outline-danger"}`}
        onClick={() => setLang("en")}
      >
        {t("lang.en")}
      </button>
    </div>
  );
}
