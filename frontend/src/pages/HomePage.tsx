import { useTranslation } from "react-i18next";
import { Link } from "react-router-dom";

export default function HomePage() {
  const { t } = useTranslation();

  return (
    <div className="text-center py-5">
      <div className="display-4 display-title">{t("home.welcomeTop")}</div>
      <div className="display-3 display-title mb-3">{t("home.welcomeBottom")}</div>
      <div className="fs-5 mb-5">{t("home.subtitle")}</div>

      <div className="container">
        <div className="row g-4 justify-content-center">
          <div className="col-12 col-md-4 col-lg-3">
            <Link to="/lobby" className="btn btn-outline-danger w-100 py-3">
              {t("home.quickJoin")}
            </Link>
          </div>
          <div className="col-12 col-md-4 col-lg-3">
            <Link to="/ai" className="btn btn-outline-danger w-100 py-3">
              {t("home.vsAi")}
            </Link>
          </div>
          <div className="col-12 col-md-4 col-lg-3">
            <Link to="/create" className="btn btn-outline-danger w-100 py-3">
              {t("home.createRoom")}
            </Link>
          </div>
          <div className="col-12 col-md-8 col-lg-6">
            <Link to="/me" className="btn btn-outline-dark w-100 py-3">
              {t("home.myWorld")}
            </Link>
          </div>
        </div>
      </div>
    </div>
  );
}
