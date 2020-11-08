import numpy as np
import re
import sklearn.linear_model
import sklearn.svm

pattern = '^\\[(..),(..),(..),(..),(..)\\],\\[(\\d+)\\]$'

f = open('twoToSF.txt')
lines = map(lambda line: re.match(pattern, line), f.readlines())
f.close()

hands = [(tuple(line[i] for i in [1, 2, 3, 4, 5]), int(line[6]), line[0]) for line in lines]

# SF Draw has cards U, V
# Other cards are X, Y, Z
# Input vector:
# [ 0- 9]: X = 3-Q
# [10-19]: Y = 4-K
# [20-29]: Z = 5-A
# [30]: X and Y match suits
# [31]: X and Z match suits
# [32]: Y and Z match suits
# [33]: X and UV match suits
# [34]: Y and UV match suits
# [35]: Z and UV match suits
def make_input_and_output(hand, c1, c2):
    x = np.zeros(36)
    suit = {'..23456789TJQKA'.find(card[0]): card[1] for card in hand[0] if card[0] not in [c1, c2]}
    X, Y, Z = sorted(suit.keys())
    S = hand[2][hand[2].find(c1) + 1]
    x[X - 3] += 1
    x[Y + 6] += 1
    x[Z + 15] += 1
    suit_checks = [(X, Y), (X, Z), (Y, Z)]
    for i, (a, b) in enumerate(suit_checks):
        if suit[a] == suit[b]:
            x[30 + i] += 1
    for i, a in enumerate([X, Y, Z]):
        if suit[a] == S:
            x[33 + i] += 1
    y = 1 if hand[1] else 0
    return (x, y)

h6, h7, h8, h9 = [], [], [], []
for hand in hands:
    if re.search('6(.),7\\1', hand[2]):
        h6.append(hand)
    if re.search('7(.),8\\1', hand[2]):
        h7.append(hand)
    if re.search('8(.),9\\1', hand[2]):
        h8.append(hand)
    if re.search('9(.),T\\1', hand[2]):
        h9.append(hand)

X6, Y6 = zip(*map(lambda h: make_input_and_output(h, '6', '7'), h6))
X6, Y6 = np.array(X6), np.array(Y6)
m6 = sklearn.linear_model.Perceptron(verbose=True)
m6.fit(X6, Y6)

X7, Y7 = zip(*map(lambda h: make_input_and_output(h, '7', '8'), h7))
X7, Y7 = np.array(X7), np.array(Y7)
m7 = sklearn.linear_model.Perceptron(verbose=True)
m7.fit(X7, Y7)

X8, Y8 = zip(*map(lambda h: make_input_and_output(h, '8', '9'), h8))
X8, Y8 = np.array(X8), np.array(Y8)
m8 = sklearn.linear_model.Perceptron(verbose=True)
m8.fit(X8, Y8)

X9, Y9 = zip(*map(lambda h: make_input_and_output(h, '9', 'T'), h9))
X9, Y9 = np.array(X9), np.array(Y9)
m9 = sklearn.linear_model.Perceptron(verbose=True)
m9.fit(X9, Y9)

print()

print(m6.score(X6, Y6))
print(m7.score(X7, Y7))
print(m8.score(X8, Y8))
print(m9.score(X9, Y9))

def test_vector(v, X, Y):
    pos = np.array([x for (x, y) in zip(X, Y) if y >= 0.5])
    neg = np.array([x for (x, y) in zip(X, Y) if y < 0.5])
    print('Y: %f ~ %f' % (min(v.dot(pos.T)[0]), max(v.dot(pos.T)[0])))
    print('N: %f ~ %f' % (min(v.dot(neg.T)[0]), max(v.dot(neg.T)[0])))
