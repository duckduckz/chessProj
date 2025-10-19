import { Routes, Route, NavLink } from "react-router-dom";
import { useTranslation } from "react-i18next";
import LanguageSwitcher from "./components/LanguageSwitcher";
import HomePage from "./pages/HomePage";
import logo from "./assets/logo.png";

export default function App() {
  const { t } = useTranslation();

  return (
    <>
      <nav className="navbar navbar-expand-lg px-3 shadow-sm">
        {/* Brand */}
        <NavLink to="/" className="navbar-brand p-0">
          <div className="d-flex align-items-center">
            <img src={logo} alt="Xiangqi World" className="brand-mark" />
            <div className="brand-text ms-2">
              <div className="brand-cn">象棋天地</div>
              <div className="brand-en">Xiangqi World</div>
            </div>
          </div>
        </NavLink>

        {/* Nav links */}
        <div className="flex-grow-1 d-flex justify-content-center">
          <ul className="navbar-nav gap-3">
            <li className="nav-item">
                <NavLink className="nav-link" to="/">{t("nav.home")}</NavLink>
            </li>
            <li className="nav-item">
                <NavLink className="nav-link" to="/lobby">{t("nav.lobby")}</NavLink>
            </li>
            <li className="nav-item">
                <NavLink className="nav-link" to="/rank">{t("nav.rank")}</NavLink>
            </li>
            <li className="nav-item">
                <NavLink className="nav-link" to="/chat">{t("nav.chat")}</NavLink>
            </li>
          </ul>
        </div>


        {/* right actions */}
        <div className="d-flex align-items-center gap-3">
          <LanguageSwitcher />
          <NavLink className="nav-link" to="/login">{t("nav.login")}</NavLink>
          <NavLink className="nav-link" to="/signup">{t("nav.signup")}</NavLink>
        </div>
      </nav>

      <div className="container py-4">
        <Routes>
          <Route path="/" element={<HomePage />} />
          {/* placeholders */}
          <Route path="/lobby"  element={<div className="text-center py-5">Lobby (coming soon)</div>} />
          <Route path="/rank"   element={<div className="text-center py-5">Ranking (coming soon)</div>} />
          <Route path="/chat"   element={<div className="text-center py-5">World Chat (coming soon)</div>} />
          <Route path="/ai"     element={<div className="text-center py-5">AI Mode (coming soon)</div>} />
          <Route path="/create" element={<div className="text-center py-5">Create Room (coming soon)</div>} />
          <Route path="/me"     element={<div className="text-center py-5">My World (coming soon)</div>} />
          <Route path="/login"  element={<div className="text-center py-5">Login (coming soon)</div>} />
          <Route path="/signup" element={<div className="text-center py-5">Sign Up (coming soon)</div>} />
        </Routes>
      </div>
    </>
  );
}
