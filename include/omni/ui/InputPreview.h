/* Copyright (c) 2014-2016 "Omnidome" by cr8tr
 * Dome Mapping Projection Software (http://omnido.me).
 * Omnidome was created by Michael Winkelmann aka Wilston Oreo (@WilstonOreo)
 *
 * This file is part of Omnidome.
 *
 * Omnidome is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Affero General Public License as
 * published by the Free Software Foundation, either version 3 of the
 * License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Affero General Public License for more details.
 * You should have received a copy of the GNU Affero General Public License
 * along with this program. If not, see <http://www.gnu.org/licenses/>.
 */

#ifndef OMNI_UI_INPUTPREVIEW_H_
#define OMNI_UI_INPUTPREVIEW_H_

#include <omni/input/Interface.h>
#include <omni/ui/GLView.h>

namespace omni
{
  namespace ui
  {
    class InputPreview : public GLView
    {
      Q_OBJECT
    public:
      InputPreview(QWidget* = nullptr);
      InputPreview(input::Interface* _input, QWidget* = nullptr);
      ~InputPreview();

      /// Return relative border value
      float border() const;

      input::Interface* input();
      input::Interface const* input() const;
      void setInput(input::Interface*);


      /**@brief Transforms mouse position on widget to normalized position on input
       * @detail Position in range (0.0,0.0) and (1.0,1.0)
       **/
      QPointF screenPos(QPointF const& _pos) const;

    public slots:
      /// Set relative border distance
      void setBorder(float);

    protected:
      void paintGL();
    signals:
      void inputChanged();

    private:
      QRectF viewRect() const;

      bool initialize();

      /// Relative border
      float border_ = 0.0;

      input::Interface* input_ = nullptr;
    };
  }
}

#endif /* OMNI_UI_INPUTPREVIEW_H_ */
