using NUnit.Framework;
using Daifugo.Data;
using UnityEngine;

namespace Daifugo.Tests.Data
{
    /// <summary>
    /// Tests for GameRulesSO class
    /// Validates runtime rule configuration and modification
    /// </summary>
    public class GameRulesSOTests
    {
        private GameRulesSO rules;

        /// <summary>
        /// Sets up test fixtures before each test
        /// </summary>
        [SetUp]
        public void Setup()
        {
            rules = ScriptableObject.CreateInstance<GameRulesSO>();
        }

        /// <summary>
        /// Cleans up after each test
        /// </summary>
        [TearDown]
        public void Teardown()
        {
            if (rules != null)
            {
                Object.DestroyImmediate(rules);
            }
        }

        #region Default Values Tests

        /// <summary>
        /// Test: Default values are set correctly
        /// </summary>
        [Test]
        public void DefaultValues_SetCorrectly()
        {
            // Assert - Implemented rules
            Assert.IsTrue(rules.Is8CutEnabled, "8-cut should be enabled by default");
            Assert.IsTrue(rules.IsBindEnabled, "Bind should be enabled by default");

            // Assert - Unimplemented rules
            Assert.IsFalse(rules.IsRevolutionEnabled, "Revolution should be disabled by default (未実装)");
            Assert.IsFalse(rules.IsSpade3ReturnEnabled, "Spade-3 return should be disabled by default (未実装)");
        }

        #endregion

        #region Setter Tests

        /// <summary>
        /// Test: Set8Cut modifies 8-cut enabled state
        /// </summary>
        [Test]
        public void Set8Cut_ChangesValue_UpdatesProperty()
        {
            // Arrange
            Assert.IsTrue(rules.Is8CutEnabled, "Initial state should be true");

            // Act
            rules.Set8Cut(false);

            // Assert
            Assert.IsFalse(rules.Is8CutEnabled, "Should be disabled after Set8Cut(false)");

            // Act again
            rules.Set8Cut(true);

            // Assert
            Assert.IsTrue(rules.Is8CutEnabled, "Should be enabled after Set8Cut(true)");
        }

        /// <summary>
        /// Test: SetBind modifies bind enabled state
        /// </summary>
        [Test]
        public void SetBind_ChangesValue_UpdatesProperty()
        {
            // Arrange
            Assert.IsTrue(rules.IsBindEnabled, "Initial state should be true");

            // Act
            rules.SetBind(false);

            // Assert
            Assert.IsFalse(rules.IsBindEnabled, "Should be disabled after SetBind(false)");

            // Act again
            rules.SetBind(true);

            // Assert
            Assert.IsTrue(rules.IsBindEnabled, "Should be enabled after SetBind(true)");
        }

        /// <summary>
        /// Test: SetRevolution modifies revolution enabled state
        /// </summary>
        [Test]
        public void SetRevolution_ChangesValue_UpdatesProperty()
        {
            // Arrange
            Assert.IsFalse(rules.IsRevolutionEnabled, "Initial state should be false");

            // Act
            rules.SetRevolution(true);

            // Assert
            Assert.IsTrue(rules.IsRevolutionEnabled, "Should be enabled after SetRevolution(true)");

            // Act again
            rules.SetRevolution(false);

            // Assert
            Assert.IsFalse(rules.IsRevolutionEnabled, "Should be disabled after SetRevolution(false)");
        }

        /// <summary>
        /// Test: SetSpade3Return modifies spade-3 return enabled state
        /// </summary>
        [Test]
        public void SetSpade3Return_ChangesValue_UpdatesProperty()
        {
            // Arrange
            Assert.IsFalse(rules.IsSpade3ReturnEnabled, "Initial state should be false");

            // Act
            rules.SetSpade3Return(true);

            // Assert
            Assert.IsTrue(rules.IsSpade3ReturnEnabled, "Should be enabled after SetSpade3Return(true)");

            // Act again
            rules.SetSpade3Return(false);

            // Assert
            Assert.IsFalse(rules.IsSpade3ReturnEnabled, "Should be disabled after SetSpade3Return(false)");
        }

        #endregion

        #region Multiple Rules Tests

        /// <summary>
        /// Test: Multiple rule changes work independently
        /// </summary>
        [Test]
        public void SetMultipleRules_WorksIndependently()
        {
            // Act
            rules.Set8Cut(false);
            rules.SetBind(false);
            rules.SetRevolution(true);
            rules.SetSpade3Return(true);

            // Assert
            Assert.IsFalse(rules.Is8CutEnabled, "8-cut should be disabled");
            Assert.IsFalse(rules.IsBindEnabled, "Bind should be disabled");
            Assert.IsTrue(rules.IsRevolutionEnabled, "Revolution should be enabled");
            Assert.IsTrue(rules.IsSpade3ReturnEnabled, "Spade-3 return should be enabled");
        }

        #endregion
    }
}
