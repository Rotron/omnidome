#include <omni/canvas/Box.h>

#include <omni/visual/Box.h>

namespace omni
{
  namespace canvas
  {
    Box::Box() : 
      vizBox_(bounds_)
    {
      setSize(QVector3D(10,10,10));
    }

    Box::~Box() 
    {
    }

    void Box::draw() const
    {
      vizBox_.draw(); 
    }

    void Box::update() 
    {
      vizBox_.update();
    }

    QVector3D Box::size() const
    {
      return this->bounds_.size();
    }

    void Box::setSize(QVector3D const& _s) 
    {
      bounds_.setMinMax(
          QVector3D(-_s.x()*0.5,-_s.y()*0.5,0.0),
          QVector3D( _s.x()*0.5, _s.y()*0.5,_s.z()));
      update();
    } 

    void Box::fromStream(QDataStream& _stream)
    {
      QVector3D _size;
      _stream >> _size;
      setSize(_size);
    }
    
    void Box::toStream(QDataStream& _stream) const
    {
      _stream << size();
    }
  }
}
