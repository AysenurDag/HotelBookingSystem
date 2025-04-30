import React from "react";
import "./Topbar.css";

const TopBar = () => {
  return (
    <div className="topbar">
      <div className="topbar-left">🌍 EN - TL</div>
      <div className="topbar-right">
        <a href="#help">Help</a>
        <a href="#currency">Currency: TRY ₺</a>
      </div>
    </div>
  );
};

export default TopBar;
