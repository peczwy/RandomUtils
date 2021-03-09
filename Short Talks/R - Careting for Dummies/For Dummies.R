## Język który tu poruszamy to R
## Jedna z dystrybucji (polecana) to: https://mran.microsoft.com/open
## Druga to: https://www.r-project.org/
## Do korzystania z R warto użyć: https://www.rstudio.com/products/rstudio/#Desktop

### Najistotniejsza rzecz: CTRL + SPACE
### Istotne wyrażenie to:
??coś
?funkcja

# Generowanie zakresu
1:20

# Proceduralne sumowanie
x <- 0
for(i in 1:20){x <- x + i}

# Prawdziwe sumowanie w R 
sum(1:20)

# Prosta funkcja
x <- function(y){return(y^2)}
x(4)

# Dynamiczny R
eval(parse(text = "x(pi^5)"))

# Sprawdzenie typu zmiennej
class(1:10)
class(list())
class(data.frame())
class(factor()) # dyskretny zbiór danych np. Species w iris 
class(matrix()) # macierze

# konstrukcja wektorów:
c(1,2,3,"TEST")

# Data frame to zbiór danych z nazwanymi kolumnami
testFrame <- data.frame(x = c(1,1), y = c(5,11), z = c("A","DSSDDS"))

# Dobieramy się do niego przez $
testFrame$x # Zwraca wektor integer
testFrame$y # Zwraca wektor integer
testFrame$z # Zwraca wektor factor

# Usuwanie zmiennej
rm(testFrame)

# Rysowanie wykresu
plot(x)

# W R jest domyślnie kilka zbiorów danych np.
iris
mtcars

# Można wyświetlić tylko początek
head(iris)

# Liczenie zestawień: tzw. analiza eksploracyjna (Exploratory (Data) Analysis)
summary(iris)

# Instalowanie pakietów
install.packages("ggplot2") # GGPlot - do rysowanie pięknych wykresów, tylko raz

# Wczytanie biblioteki:
library(ggplot2)

# Piękny wykres Iris
ggplot(data = iris, aes(x = Petal.Length, y = Petal.Width, color = Species)) + geom_point()

## Ogólnie rzecz biorąc, istnieją trzy (nieprawda) bibioteki do rysowania wykresów: plot(), lattice(), ggplot()
## W JS's istnieje plotly

### Caret
install.packages("caret") # Fasada dla machine-learningu - tylko raz
library("caret")

#Jak puszczałem LVQ to Caret poprosił o ręczne zainstalowanie pakietu o śmiesznej nazwie coś w stylu: install.packages("e90311")

# Najważniejsza metoda to:
names(getModelInfo())

# Sprawdzenie metody konkrentej np. LVQ
getModelInfo("lvq")
## Ten fragment jest istotny
## $lvq$parameters
##  parameter   class         label
##      size numeric Codebook Size
##         k numeric   #Prototypes

### Prosty przykład klasyfikacji - w sumie tak nie rób
##  Głownie przez to: Resampling: Bootstrapped (25 reps)
model <- train(Species ~ Petal.Length + Petal.Width, data = iris, method = "lvq")
newData <- data.frame(Petal.Length = c(2, 1, 3, 5, 7), Petal.Width = c(0.5, 0.25, 1, 1.5, 2.5))

# To nie istotne - to do produkcji 
predict(model, newData)

### Trochę bardziej skomplikowany
expand.grid(size = c(1,2,3), k = 1:10) # jako generowanie wszystko X wszystko

# Korzystamy tak:
grid <- expand.grid(size = 3:10, k = 1:10)
model <- train(Species ~ Petal.Length + Petal.Width, data = iris, method = "lvq", tuneGrid = grid)

# Zmienimy metodę doboru zbioru uczącego:
control <- trainControl(method = "repeatedcv", repeats = 10)
model <- train(Species ~ Petal.Length + Petal.Width, data = iris, method = "lvq", trControl = control)

# Najlepiej robić tak:
grid <- expand.grid(size = 3:10, k = 1:10)
control <- trainControl(method = "repeatedcv", repeats = 10)
model <- train(Species ~ Petal.Length + Petal.Width, data = iris, method = "lvq", tuneGrid = grid, trControl = control)

# Ocena modelu na podstawie macierzy konfuzji:
confusionMatrix(model)
confusionMatrix(model, norm = "none")

### Jeszcze docisnąć i zmienić metodę
getModelInfo("C5.0")
control <- trainControl(method = "repeatedcv", repeats = 10)
model <- train(Species ~ Petal.Length + Petal.Width, data = iris, method = "C5.0",trControl = control

### Można zastosować nie klasyfikację, ale regresję
model <- train(Petal.Width ~ Petal.Length, data = iris, method = "lm", trControl = control) #LM to model liniowy (Linear Model)
newData <- data.frame(Petal.Length = 1:7)
predict(model, newData)

#### Jak wczytać dane?
dat <- read.csv(file = "...../0_41_sredniaasortymenty_material.csv", header =  TRUE)


### Biblioteki
install.packages("dplyr")
library(dplyr)

# Po załadowaniu możesz korzystać z niesamowitych funkcji w ramach data.frame
# Najlepiej łącząc operacje za pomocą %>%
dat %>% filter(length_min < 20) %>% select(length_min) %>% mutate(X = length_min * 2)

## Dziwactwa w R
c() # konkatenacja wektora
paste() # konkatenacja stringa
sd() # odchylenie
var() # variancja czyli var() = sd()^2
quantile(1:10, c(0.01, 0.99)) # Wybranie kwantyli 0.01 i 0.99

# Ultra lamerski przykład zastosowania eval:
template <- "model <- train(Species ~"
template_2 <- ", data = iris, method = \"lvq\")"
f <- function(y){paste(template, paste(sample(x = colnames(iris)[1:4], size = y, replace = FALSE), collapse = "+"), template_2, sep = "" , collapse = "")}
eval(parse(text = f(4)))