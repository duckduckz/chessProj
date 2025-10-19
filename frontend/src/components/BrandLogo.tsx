import { NavLink } from "react-router-dom";
import logo from "../assets/logo.png";

export default function BrandLogo() {
    return (
        <NavLink to="/" className="navbar-brand p-0">
            <div className="brand-wrap d-flex align-items-center">
                <img src={logo} alt="Xiangqi World" className="brand-mark"/>
                <div className="brand-text ms-2">
                    <div className="brand-cn">象棋世界</div>
                    <div className="brand-en">Xiangqi World</div>
                </div>
            </div>
        </NavLink>
    )
}