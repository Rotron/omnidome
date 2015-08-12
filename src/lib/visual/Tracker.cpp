#include <omni/visual/Tracker.h>

#include <algorithm>

namespace omni
{
  namespace visual
  {
    Tracker::Tracker() :
      center_(0.0,0.0,0.0),
      direction_(0.0,0.0,1.0)
    {
    }

    Tracker::Tracker(
      const QVector3D& _center,
      const PolarVec& _direction) :
      center_(_center),
      direction_(_direction)
    {
    }

    void Tracker::track( float _longitude, float _latitude, float _radius)
    {
      direction_ += PolarVec(Angle(_longitude),Angle(_latitude),_radius);
    }
      
    QVector3D Tracker::eye() const
    {
      return center_ + direction_.vec();
    }
   
    void Tracker::setEye(const QVector3D& _pos)
    {
      direction_ = _pos - center_;
    }

    void Tracker::setCenter(QVector3D const& _center)
    {
      center_=_center;
    }

    QVector3D& Tracker::center()
    {
      return center_;
    }
    
    QVector3D const& Tracker::center() const
    {
      return center_;
    }

    void Tracker::setDirection(PolarVec const& _direction)
    {
      direction_ = _direction;
    }

    void Tracker::setDistance(float _t)
    {
      direction_ = _t * direction_.normalized();
    }

    PolarVec& Tracker::direction()
    {
      return direction_;
    }

    PolarVec const& Tracker::direction() const
    {
      return direction_;
    } 
      
    void Tracker::limitDistance(float _minDist, float _maxDist)
    {
      float _r = direction_.radius();
      if (_minDist > _maxDist)
        std::swap(_minDist,_maxDist);

      if (_r < _minDist) 
        direction_.setRadius(_minDist);
      if (_r > _maxDist) 
        direction_.setRadius(_maxDist);
    }
  }
}
