import React from "react";
import Slider from "react-slick";
import "slick-carousel/slick/slick.css";
import "slick-carousel/slick/slick-theme.css";

const bannerImages = [
  {
    url: "/images/banner1.jpg",
    alt: "Summer Deals 2025",
  },
  {
    url: "/images/banner2.jpg",
    alt: "City Getaways",
  },
];

const BannerSlider = () => {
  const settings = {
    dots: true,
    infinite: true,
    autoplay: true,
    speed: 800,
    autoplaySpeed: 4000,
    arrows: false,
  };

  return (
    <Slider {...settings}>
      {bannerImages.map((img, index) => (
        <div key={index}>
          <img
            src={img.url}
            alt={img.alt}
            style={{
              width: "100%",
              height: "400px",
              objectFit: "cover",
              borderRadius: "16px",
              display: "block",
            }}
          />
        </div>
      ))}
    </Slider>
  );
};

export default BannerSlider;
