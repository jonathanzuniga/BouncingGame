using System;
using System.Collections.Generic;
using CocosSharp;
using CocosDenshion;

namespace BouncingGame
{
	public class GameLayer : CCLayer
	{
		CCSprite paddleSprite;
		CCSprite ballSprite;
		CCLabel scoreLabel, highestScoreLabel;

		float ballXVelocity;
		float ballYVelocity;
		// How much to modify the ball's y velocity per second:
		const float gravity = 700;
		int score, highestScore;

		public GameLayer ()
		{
			// "paddle" refers to the paddle.png image.
			paddleSprite = new CCSprite ("paddle");
			paddleSprite.PositionX = 180;
			paddleSprite.PositionY = 100;
			AddChild (paddleSprite);

			ballSprite = new CCSprite ("ball");
			ballSprite.PositionX = 180;
			ballSprite.PositionY = 600;
			AddChild (ballSprite);

			scoreLabel = new CCLabel ("Score: 0", "arial", 28, CCLabelFormat.SystemFont);
			scoreLabel.PositionX = 50;
			scoreLabel.PositionY = 480;
			scoreLabel.AnchorPoint = CCPoint.AnchorUpperLeft;
			AddChild (scoreLabel);

			highestScoreLabel = new CCLabel ("Highest Score: 0", "arial", 28, CCLabelFormat.SystemFont);
			highestScoreLabel.PositionX = 50;
			highestScoreLabel.PositionY = 500;
			highestScoreLabel.AnchorPoint = CCPoint.AnchorUpperLeft;
			AddChild (highestScoreLabel);

			Schedule (RunGameLogic);
		}

		void RunGameLogic(float frameTimeInSeconds)
		{
			// This is a linear approximation, so not 100% accurate.
			ballYVelocity += frameTimeInSeconds * -gravity;
			ballSprite.PositionX += ballXVelocity * frameTimeInSeconds;
			ballSprite.PositionY += ballYVelocity * frameTimeInSeconds;

			// Check if the two CCSprites overlap...
			bool doesBallOverlapPaddle = ballSprite.BoundingBoxTransformedToParent.IntersectsRect(
				paddleSprite.BoundingBoxTransformedToParent);
			// ... and if the ball is moving downward.
			bool isMovingDownward = ballYVelocity < 0;

			if (doesBallOverlapPaddle && isMovingDownward) {
				// First let's invert the velocity:
				ballYVelocity *= -1;

				// Then let's assign a random value to the ball's x velocity:
				const float minXVelocity = -600;
				const float maxXVelocity = 600;

				ballXVelocity = CCRandom.GetRandomFloat (minXVelocity, maxXVelocity);
				score++;
				scoreLabel.Text = "Score: " + score;

				var effect = new CCParticleExplosion(ballSprite.Position) {
					AutoRemoveOnFinish = true,
					EmissionRate = 100
				};
				AddChild(effect);

				CCSimpleAudioEngine.SharedEngine.PlayEffect("tennis_ball");
			}

			// First let’s get the ball position:   
			float ballRight = ballSprite.BoundingBoxTransformedToParent.MaxX;
			float ballLeft = ballSprite.BoundingBoxTransformedToParent.MinX;
			// Then let’s get the screen edges.
			float screenRight = VisibleBoundsWorldspace.MaxX;
			float screenLeft = VisibleBoundsWorldspace.MinX;
			// Check if the ball is either too far to the right or left:    
			bool shouldReflectXVelocity = 
				(ballRight > screenRight && ballXVelocity > 0) ||
				(ballLeft < screenLeft && ballXVelocity < 0);
			
			if (shouldReflectXVelocity) {
				ballXVelocity *= -1;
			}

			if (ballSprite.PositionY < VisibleBoundsWorldspace.MinY) {
				ballSprite.PositionY = VisibleBoundsWorldspace.MaxY;
				if (highestScore < score) {
					highestScore = score;
					highestScoreLabel.Text = "Highest Score: " + highestScore;
				}

				score = 0;
				scoreLabel.Text = "Score: 0";
				ballXVelocity = 0;
				ballYVelocity = 0;
			}
		}

		protected override void AddedToScene ()
		{
			base.AddedToScene ();

			// Use the bounds to layout the positioning of our drawable assets.
			CCRect bounds = VisibleBoundsWorldspace;

			// Register for touch events.
			var touchListener = new CCEventListenerTouchAllAtOnce ();

			touchListener.OnTouchesEnded = OnTouchesEnded;
			touchListener.OnTouchesMoved = HandleTouchesMoved;

			AddEventListener (touchListener, this);
		}

		void HandleTouchesMoved (System.Collections.Generic.List<CCTouch> touches, CCEvent touchEvent)
		{
			// we only care about the first touch:
			var locationOnScreen = touches [0].Location;

			paddleSprite.PositionX = locationOnScreen.X;
		}

		void OnTouchesEnded (List<CCTouch> touches, CCEvent touchEvent)
		{
			if (touches.Count > 0) {
				// Perform touch handling here.
			}
		}
	}
}
