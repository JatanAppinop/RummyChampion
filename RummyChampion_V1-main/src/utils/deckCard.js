const suits = ["HEARTS", "DIAMONDS", "CLUBS", "SPADES"];
const values = [
  "ACE",
  "2",
  "3",
  "4",
  "5",
  "6",
  "7",
  "8",
  "9",
  "10",
  "JACK",
  "QUEEN",
  "KING",
];

// Function to create a single deck with one fixed Joker and one wild Joker
function createDeck() {
  let deck = suits.flatMap((suit) =>
    values.map((value) => {
      let code = value[0];
      if (value === "10") code = "0"; // Special case for 10
      if (value === "JACK") code = "J";
      if (value === "QUEEN") code = "Q";
      if (value === "KING") code = "K";
      if (value === "ACE") code = "A";

      return {
        value,
        suit,
        code: `${code}${suit[0]}`, // Example: 10H, KH, QS
      };
    })
  );

  // Add two Jokers (one is a regular Joker, the other will be assigned as Wild)
  deck.push({ value: "JOKER", suit: "BLACK", code: "X1" }); // Regular Joker
  deck.push({ value: "JOKER", suit: "RED", code: "X2" }); // This can be wild

  return deck;
}

// Function to create multiple decks
function createMultipleDecks(deckCount) {
  let fullDeck = [];
  for (let i = 0; i < deckCount; i++) {
    fullDeck.push(...createDeck());
  }
  return fullDeck;
}

// Function to shuffle the deck using Fisher-Yates algorithm
function shuffleDeck(deck) {
  for (let i = deck.length - 1; i > 0; i--) {
    const j = Math.floor(Math.random() * (i + 1));
    [deck[i], deck[j]] = [deck[j], deck[i]]; // Swap elements
  }
  return deck;
}

// Function to randomly assign a Wild Joker (excluding existing jokers)
function selectWildJoker(deck) {
  // Get all non-joker cards
  const nonJokerCards = deck.filter(
    (card) => card.code !== "X1" && card.code !== "X2"
  );

  // Randomly pick a Wild Card Joker
  const randomIndex = Math.floor(Math.random() * nonJokerCards.length);
  const wildJoker = nonJokerCards[randomIndex];

  // Mark it as a Wild Joker
  wildJoker.isWildJoker = true;

  return wildJoker;
}

// Function to create and shuffle decks with a wild joker
function newDeck(deckCount = 1) {
  let deck = createMultipleDecks(deckCount);
  deck = shuffleDeck(deck);

  // Select a wild joker
  const wildJoker = selectWildJoker(deck);

  return {
    deck_id: Math.random().toString(36).substr(2, 12),
    cards: deck,
    wildJoker, // Return the wild joker information
  };
}

// Function to reshuffle an existing deck (keeping the same wild joker)
function reshuffleDeck(deck) {
  return {
    cards: shuffleDeck([...deck]), // Clone and shuffle
  };
}

// Export the functions using CommonJS
module.exports = {
  createDeck,
  createMultipleDecks,
  shuffleDeck,
  selectWildJoker,
  newDeck,
  reshuffleDeck,
};
